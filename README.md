# OWASP-10-A09-security-logging-and-monitoring-failures

Challenging well-known, widely supported log libraries in .NET against CWE-117: Improper Output Neutralization for Logs threat

-----

This repository contains a security lab built to demonstrate and test **CWE-117 (Improper Output Neutralization for Logs)** against the most popular .NET logging libraries.

The experiment is based on the 9th section of the OWASP Top 10 list, "A09:2021 – Security Logging and Monitoring Failures," and a specific CVE related to improper output neutralization.

## 🧪 Lab Overview

The goal of this lab is to test how user-supplied input is handled by common .NET loggers when it contains malicious control characters designed to forge or corrupt log data.

  * **Vulnerability:** [CWE-117: Improper Output Neutralization for Logs](https://cwe.mitre.org/data/definitions/117.html)
  * **Framework:** .NET Core
  * **Libraries Tested:**
      * Serilog
      * NLog
      * Log4net
  * **Setup:** A minimal .NET Web API that passes a JSON request body directly into a logger, simulating a real-world application flow.

-----

## 📖 Understanding CWE-117: Improper Output Neutralization

CWE-117 describes a vulnerability where an application writes external input (e.g., from a user, an API, or another service) to a log file without first sanitizing or "neutralizing" special elements within that input.

An attacker can exploit this by crafting a payload that, when logged, corrupts the log file's integrity. The common consequences include:

  * **Log Forging:** Injecting fake log entries to mislead administrators, cover an attacker's tracks, or implicate another user.
  * **Log Corrupton:** Breaking the format of the log file (e.g., in a TSV or JSON log), which can cause automated parsing tools to fail.
  * **Command Injection:** In a more severe scenario, an attacker might inject commands. This is particularly dangerous if a **log processing utility** (not the logger itself) is vulnerable and executes or interprets data from the logs.

As the CWE documentation states:

> "An attacker may inject code or other commands into the log file and take advantage of a vulnerability in the log processing utility."

-----

## 🔬 Lab Environment Setup

The environment consists of a simple .NET Web API project with a single endpoint. This scaffolding is replicated for each of the three libraries (Serilog, NLog, Log4net).

The application simulates a "bodybuilding" app and has an endpoint to create a new routine. The vulnerable part is that it logs the user-supplied `Description` field for an exercise.

### API Endpoint (`RoutinesController.cs`)

This is the entry point that receives the HTTP POST request and its JSON body.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using owasp._09.serilog.core.UseCases.CreateRoutine.Contracts;

namespace owasp._09.serilog.api.Endpoints.CreateRoutine;

[ApiController]
[Route("[controller]")]
public class RoutinesController : ControllerBase
{
    private readonly ICreateRoutineHandler _handler;

    public RoutinesController(ICreateRoutineHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRoutine(int id, [FromBody] CreateRoutineRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = new CreateRoutineInput()
        {
            Name = request.Name,
            Type = request.Type,
            Exercises = request.Exercises.Select(x =>
                new core.UseCases.CreateRoutine.Contracts.ExerciseDetails()
                {
                    Name = x.Name,
                    Region = x.Region,
                    Repetitions = x.Repetitions,
                    Description = x.Description
                }
            ).ToList()
        };

        await _handler.Handle(input);

        return new OkObjectResult(id);
    }
}
```

### Business Logic (`CreateRoutineHandler.cs`)

This handler contains the vulnerable logging call. The `Description` property, which comes directly from the user's request, is passed to the logger.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using owasp._09.serilog.core.UseCases.CreateRoutine.Contracts;

namespace owasp._09.serilog.core.UseCases.CreateRoutine;

public sealed class CreateRoutineHandler : ICreateRoutineHandler
{
    private readonly ILogger<CreateRoutineHandler> _logger;

    public CreateRoutineHandler(ILogger<CreateRoutineHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CreateRoutineInput request)
    {
        foreach (var exercise in request.Exercises)
        {
            // VULNERABLE CALL: The 'Description' comes directly from user input
            _logger.LogInformation("Description: {Description}", exercise.Description);
        }
    }
}
```

-----

## 🎯 Executing the Injection Attack

The experiment involved two phases, which highlight a crucial detail about `Content-Type` and how payloads must be crafted.

### Attempt 1: The URL-Encoded Payload (Failed)

Based on the OWASP demonstrative example, the first attempt used a URL-encoded payload. This is common for attacks via query parameters or `application/x-www-form-urlencoded` forms.

  * **Payload:** `twenty-one%0a%0aINFO:+User+logged+out%3dbadguy`
  * **Sent as:** The value of the `description` field in the JSON request body.

**Result: FAILURE**

The log file showed that the string was **not** interpreted as control characters. It was logged literally.

```log
2025-11-15 12:13:24.533 +01:00 [INF] [owasp._09.serilog.core.UseCases.CreateRoutine.CreateRoutineHandler] Description: twenty-one%0a%0aINFO:+User+logged+out%3dbadguy
2025-11-15 12:13:24.540 +01:00 [INF] [owasp._09.serilog.core.UseCases.CreateRoutine.CreateRoutineHandler] Description: twenty-one%0a%0aINFO:+User+logged+out%3dbadguy
```

**Analysis:** The attack failed because the endpoint's **`Content-Type` is `application/json`**. The .NET JSON deserializer does not parse URL-encoded characters (like `%0a`) within a string value. Instead, it correctly (and safely) treats them as literal text. The logger never saw the newline character, only the string "%0a".

### Attempt 2: JSON Escape Sequences (Success)

The logical next step was to use escape sequences that the **JSON format itself** interprets. The most common for newlines are `\r` (Carriage Return) and `\n` (Line Feed).

  * **Payload (inside JSON):**
    ```json
    {
      "description": "twenty-one\r\nINFO: User logged out bad guy"
    }
    ```

**Result: SUCCESS**

This time, the JSON deserializer parsed `\r\n` into actual newline characters. When the logger received this string, its file sink interpreted them as instructions to create new lines, successfully forging a log entry.

```log
2025-11-15 13:03:30.029 +01:00 [INF] [owasp._09.serilog.core.UseCases.CreateRoutine.CreateRoutineHandler] Description: twenty-one
INFO: User logged out bad guy
2025-11-15 13:03:30.036 +01:00 [INF] [owasp._09.serilog.core.UseCases.CreateRoutine.CreateRoutineHandler] Description: twenty-one
INFO: User logged out bad guy
```

**Conclusion:** This test confirms that **Serilog, NLog, and Log4net** can all be vulnerable to log forging when they are fed un-sanitized input containing standard newline characters, even when using structured logging templates.

-----

## 🚀 Further Research & Potential Attacks

This lab serves as a starting point for more advanced research.

### Potential for Command Execution

As noted from the CWE-117 documentation, the most severe risk is the injection of code or commands. It's important to understand that this attack **targets the log processing utility, not the logger itself.**

Imagine a scenario where:

1.  An attacker injects a payload (e.g., HTML, JavaScript, or a shell command) into your logs.
2.  The log file is consumed by a secondary tool:
      * A **web-based dashboard** (like Kibana, Splunk, or a custom panel) that renders log entries as HTML. An injected `<script>` tag could lead to XSS, stealing an admin's session cookie.
      * A **cron job or script** that parses the log file. If this script uses a tool like `awk` or `sed` in an insecure way, an injected payload might be able to trigger command execution.

Future tests could involve setting up such a vulnerable log processor to demonstrate this full attack chain.

### Other Vectors

  * **Unicode Characters:** The `application/json` spec also interprets Unicode sequences (`\uXXXX`). This could be another vector for bypassing simple filters.
  * **Log Vocabulary:** The [OWASP Logging Vocabulary Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Vocabulary_Cheat_Sheet.html) provides excellent guidelines for crafting more convincing and subtle fake log messages.

-----

## 📂 Repository Details

The full implementation for all three libraries (Serilog, NLog, Log4net) can be found in this repository:

[https://github.com/sharp-circles/OWASP-10-A09-security-logging-and-monitoring-failures](https://github.com/sharp-circles/OWASP-10-A09-security-logging-and-monitoring-failures)

The `output-files` folder contains the raw log files generated during these tests.
