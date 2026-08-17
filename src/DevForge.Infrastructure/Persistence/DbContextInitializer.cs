using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DevForge.Domain.Entities;

namespace DevForge.Infrastructure.Persistence;

/// <summary>
/// Handles database migration and seeding operations on application startup.
/// </summary>
public class DbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DbContextInitializer> _logger;

    public DbContextInitializer(ApplicationDbContext context, ILogger<DbContextInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsSqlite())
            {
                await _context.Database.EnsureCreatedAsync();
            }
            else if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // 1. Seed Roles
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role { Name = "Admin" };
            _context.Roles.Add(adminRole);
        }

        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
        {
            userRole = new Role { Name = "User" };
            _context.Roles.Add(userRole);
        }

        await _context.SaveChangesAsync();

        // 2. Seed Admin User
        var adminEmail = "admin@devforge.com";
        var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                Username = "admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!")
            };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
            await _context.SaveChangesAsync();
        }

        // 3. Seed Normal User
        var userEmail = "user@devforge.com";
        var normalUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (normalUser == null)
        {
            normalUser = new User
            {
                Username = "user",
                Email = userEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!")
            };
            _context.Users.Add(normalUser);
            await _context.SaveChangesAsync();

            _context.UserRoles.Add(new UserRole { UserId = normalUser.Id, RoleId = userRole.Id });
            await _context.SaveChangesAsync();
        }

        // 4. Seed Interview Questions
        if (!await _context.Questions.AnyAsync())
        {
            await SeedQuestionsAsync();
        }
    }

    private async Task SeedQuestionsAsync()
    {
        var questions = new List<Question>
        {
            // ── C# ─────────────────────────────────────────────────────────────────

            new Question
            {
                Text = "Which dependency injection lifetime creates a new service instance for every single request made to the service?",
                Category = QuestionCategory.CSharp,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "Transient lifetime creates a new instance every time the service is requested. This is suitable for lightweight, stateless services.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Singleton", IsCorrect = false },
                    new QuestionOption { Text = "Scoped", IsCorrect = false },
                    new QuestionOption { Text = "Transient", IsCorrect = true },
                    new QuestionOption { Text = "Pooled", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is the key difference between a struct and a class in C#?",
                Category = QuestionCategory.CSharp,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "A struct is a value type stored on the stack (or inline), while a class is a reference type stored on the heap. Structs are copied on assignment; classes share the same reference.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Structs cannot have methods", IsCorrect = false },
                    new QuestionOption { Text = "Structs are value types; classes are reference types", IsCorrect = true },
                    new QuestionOption { Text = "Structs support inheritance; classes do not", IsCorrect = false },
                    new QuestionOption { Text = "Structs are always stored on the heap", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What does the 'await' keyword do in an async method?",
                Category = QuestionCategory.CSharp,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "The 'await' keyword suspends execution of the current async method until the awaited Task completes, freeing the calling thread in the meantime rather than blocking it.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Blocks the current thread until the task finishes", IsCorrect = false },
                    new QuestionOption { Text = "Creates a new background thread for the operation", IsCorrect = false },
                    new QuestionOption { Text = "Suspends the method and returns control to the caller until the task completes", IsCorrect = true },
                    new QuestionOption { Text = "Runs the task in parallel without waiting", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "Which SOLID principle states that a class should have only one reason to change?",
                Category = QuestionCategory.CSharp,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "The Single Responsibility Principle (SRP) states that a class should have only one job or reason to change. This keeps classes focused and easier to maintain and test.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Open/Closed Principle", IsCorrect = false },
                    new QuestionOption { Text = "Single Responsibility Principle", IsCorrect = true },
                    new QuestionOption { Text = "Liskov Substitution Principle", IsCorrect = false },
                    new QuestionOption { Text = "Dependency Inversion Principle", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is the purpose of the 'IDisposable' interface in C#?",
                Category = QuestionCategory.CSharp,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "IDisposable provides a standard mechanism to release unmanaged resources (file handles, database connections, etc.) deterministically. Classes implementing it are used with 'using' statements to ensure Dispose() is called.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "To allow objects to be serialised to JSON", IsCorrect = false },
                    new QuestionOption { Text = "To mark a class as abstract", IsCorrect = false },
                    new QuestionOption { Text = "To define a deterministic mechanism for releasing unmanaged resources", IsCorrect = true },
                    new QuestionOption { Text = "To prevent garbage collection of an object", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "In C#, what is a generic type constraint 'where T : class' enforcing?",
                Category = QuestionCategory.CSharp,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "The 'where T : class' constraint restricts the type parameter T to reference types only, preventing value types like int or struct from being used as the type argument.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "T must implement a specific interface", IsCorrect = false },
                    new QuestionOption { Text = "T must be a reference type", IsCorrect = true },
                    new QuestionOption { Text = "T must have a parameterless constructor", IsCorrect = false },
                    new QuestionOption { Text = "T must be a value type", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What problem does the 'async void' pattern cause compared to 'async Task'?",
                Category = QuestionCategory.CSharp,
                Difficulty = QuestionDifficulty.Hard,
                Explanation = "With 'async void', exceptions thrown inside the method cannot be caught by the caller because there is no Task to observe. This typically causes unhandled exceptions that crash the process. 'async Task' allows callers to await and catch exceptions properly.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "It prevents the method from being awaited, causing exceptions to be silently swallowed or crash the process", IsCorrect = true },
                    new QuestionOption { Text = "It runs on a different thread pool", IsCorrect = false },
                    new QuestionOption { Text = "It blocks the UI thread", IsCorrect = false },
                    new QuestionOption { Text = "It prevents the method from returning a value", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is boxing in C# and when does it occur?",
                Category = QuestionCategory.CSharp,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "Boxing is the conversion of a value type (e.g., int) to an object or interface type, causing heap allocation. It occurs implicitly when a value type is assigned to an object variable or passed to a method expecting object.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Wrapping a class inside a struct", IsCorrect = false },
                    new QuestionOption { Text = "Converting a reference type to a value type", IsCorrect = false },
                    new QuestionOption { Text = "Converting a value type to a reference type, causing a heap allocation", IsCorrect = true },
                    new QuestionOption { Text = "Sealing a class to prevent inheritance", IsCorrect = false }
                }
            },

            // ── ASP.NET Core ───────────────────────────────────────────────────────

            new Question
            {
                Text = "In ASP.NET Core, what is the correct order in which middleware runs?",
                Category = QuestionCategory.AspNetCore,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "ASP.NET Core middleware runs in the order it is registered with 'app.Use...' in Program.cs. Each middleware can process the request, pass it to the next middleware, and then process the response on the way back out.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "In reverse order of registration", IsCorrect = false },
                    new QuestionOption { Text = "In the order they are registered in Program.cs", IsCorrect = true },
                    new QuestionOption { Text = "Alphabetically by middleware name", IsCorrect = false },
                    new QuestionOption { Text = "By priority value assigned at registration", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "Which ASP.NET Core interface allows you to run background work when the application starts and stops?",
                Category = QuestionCategory.AspNetCore,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "IHostedService allows you to implement StartAsync and StopAsync to run background work alongside the application. BackgroundService is a base class that simplifies long-running IHostedService implementations.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "IMiddleware", IsCorrect = false },
                    new QuestionOption { Text = "IApplicationLifetime", IsCorrect = false },
                    new QuestionOption { Text = "IHostedService", IsCorrect = true },
                    new QuestionOption { Text = "IStartupFilter", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is the role of the 'appsettings.Development.json' file in ASP.NET Core?",
                Category = QuestionCategory.AspNetCore,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "appsettings.Development.json overrides values in appsettings.json when the ASPNETCORE_ENVIRONMENT is set to 'Development'. This allows environment-specific configuration without modifying the base configuration file.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "It replaces appsettings.json entirely in development", IsCorrect = false },
                    new QuestionOption { Text = "It overrides appsettings.json values when running in the Development environment", IsCorrect = true },
                    new QuestionOption { Text = "It is only used during unit testing", IsCorrect = false },
                    new QuestionOption { Text = "It stores production secrets", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "In ASP.NET Core MVC, what is the difference between a Filter and Middleware?",
                Category = QuestionCategory.AspNetCore,
                Difficulty = QuestionDifficulty.Hard,
                Explanation = "Middleware runs in the request pipeline and has no knowledge of MVC concepts like actions or controllers. Filters run within the MVC pipeline and have access to action context, allowing more targeted interception of controller actions and results.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Filters run before middleware in the pipeline", IsCorrect = false },
                    new QuestionOption { Text = "Middleware is MVC-aware; filters are not", IsCorrect = false },
                    new QuestionOption { Text = "Filters are MVC-aware and can access action context; middleware operates at the HTTP pipeline level", IsCorrect = true },
                    new QuestionOption { Text = "There is no practical difference", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What does 'UseAuthentication()' vs 'UseAuthorization()' do in the ASP.NET Core pipeline?",
                Category = QuestionCategory.AspNetCore,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "UseAuthentication() identifies who the user is (sets HttpContext.User). UseAuthorization() checks whether the identified user is permitted to access the requested resource. Authentication must come before Authorization.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "UseAuthorization() identifies the user; UseAuthentication() enforces access policies", IsCorrect = false },
                    new QuestionOption { Text = "UseAuthentication() identifies the user; UseAuthorization() enforces access policies", IsCorrect = true },
                    new QuestionOption { Text = "They are interchangeable and can be registered in any order", IsCorrect = false },
                    new QuestionOption { Text = "UseAuthentication() handles role-based access control", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What does the [ValidateAntiForgeryToken] attribute protect against in ASP.NET Core MVC?",
                Category = QuestionCategory.AspNetCore,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "The [ValidateAntiForgeryToken] attribute protects POST endpoints against Cross-Site Request Forgery (CSRF) attacks by requiring a server-generated token that is unique per session to accompany form submissions.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "SQL injection attacks", IsCorrect = false },
                    new QuestionOption { Text = "Cross-Site Scripting (XSS) attacks", IsCorrect = false },
                    new QuestionOption { Text = "Cross-Site Request Forgery (CSRF) attacks", IsCorrect = true },
                    new QuestionOption { Text = "Brute force password attacks", IsCorrect = false }
                }
            },

            // ── Web API ─────────────────────────────────────────────────────────────

            new Question
            {
                Text = "Which HTTP status code should an API return when a resource is successfully created?",
                Category = QuestionCategory.WebApi,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "HTTP 201 Created is the correct status code when a new resource has been successfully created. It is often accompanied by a Location header pointing to the new resource.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "200 OK", IsCorrect = false },
                    new QuestionOption { Text = "201 Created", IsCorrect = true },
                    new QuestionOption { Text = "202 Accepted", IsCorrect = false },
                    new QuestionOption { Text = "204 No Content", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "In a RESTful API, which HTTP method is idempotent and used to fully replace a resource?",
                Category = QuestionCategory.WebApi,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "PUT is used to fully replace a resource at the specified URI and is idempotent (calling it multiple times with the same data produces the same result). PATCH is used for partial updates.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "POST", IsCorrect = false },
                    new QuestionOption { Text = "PATCH", IsCorrect = false },
                    new QuestionOption { Text = "PUT", IsCorrect = true },
                    new QuestionOption { Text = "UPDATE", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What does the [ApiController] attribute automatically provide in ASP.NET Core Web API?",
                Category = QuestionCategory.WebApi,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "[ApiController] enables automatic model state validation (returning 400 if invalid), automatic binding source inference ([FromBody], [FromQuery] etc.), and ProblemDetails error responses — removing the need for manual ModelState checks.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Automatic route prefix configuration", IsCorrect = false },
                    new QuestionOption { Text = "Automatic model state validation and binding source inference", IsCorrect = true },
                    new QuestionOption { Text = "Automatic JWT authentication", IsCorrect = false },
                    new QuestionOption { Text = "Automatic Swagger documentation generation", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What HTTP status code should be returned when a request is syntactically valid but fails business logic validation?",
                Category = QuestionCategory.WebApi,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "HTTP 422 Unprocessable Entity indicates the request was well-formed but contained semantic errors. However, 400 Bad Request is also widely accepted for validation failures. 422 is more precise for business rule violations.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "404 Not Found", IsCorrect = false },
                    new QuestionOption { Text = "500 Internal Server Error", IsCorrect = false },
                    new QuestionOption { Text = "409 Conflict", IsCorrect = false },
                    new QuestionOption { Text = "422 Unprocessable Entity", IsCorrect = true }
                }
            },

            new Question
            {
                Text = "What is the difference between authentication and authorization in API security?",
                Category = QuestionCategory.WebApi,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "Authentication verifies the identity of the caller ('who are you?'). Authorization determines what the authenticated caller is allowed to do ('what can you do?'). Both are distinct and work together to secure APIs.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "They are different names for the same process", IsCorrect = false },
                    new QuestionOption { Text = "Authentication checks permissions; authorization verifies identity", IsCorrect = false },
                    new QuestionOption { Text = "Authentication verifies identity; authorization checks what the identity is permitted to do", IsCorrect = true },
                    new QuestionOption { Text = "Authorization occurs before authentication in the pipeline", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "In JWT authentication, what part of the token should the server validate to prevent tampering?",
                Category = QuestionCategory.WebApi,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "The server must validate the JWT signature using the secret key or public key. The header and payload are Base64-encoded and readable by anyone — only the signature, produced by signing the header and payload with the secret, proves the token has not been tampered with.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "The header, because it contains the algorithm", IsCorrect = false },
                    new QuestionOption { Text = "The payload, because it contains the claims", IsCorrect = false },
                    new QuestionOption { Text = "The signature, because it proves the token has not been tampered with", IsCorrect = true },
                    new QuestionOption { Text = "The expiry claim only", IsCorrect = false }
                }
            },

            // ── EF Core ─────────────────────────────────────────────────────────────

            new Question
            {
                Text = "What does AsNoTracking() do in EF Core, and when should you use it?",
                Category = QuestionCategory.EfCore,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "AsNoTracking() tells EF Core not to track returned entities in the ChangeTracker. This improves performance for read-only queries because EF does not need to monitor the entities for changes. Use it whenever you do not need to update the entities.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "It disables lazy loading for navigation properties", IsCorrect = false },
                    new QuestionOption { Text = "It prevents EF Core from tracking entities, improving read-only query performance", IsCorrect = true },
                    new QuestionOption { Text = "It prevents the query from hitting the database", IsCorrect = false },
                    new QuestionOption { Text = "It returns entities without any navigation properties loaded", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is the N+1 query problem in EF Core?",
                Category = QuestionCategory.EfCore,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "The N+1 problem occurs when EF Core runs 1 query to retrieve a list of entities, then runs N additional queries to load a related entity for each item. The fix is to use Include() to eagerly load the related data in a single JOIN query.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Running N queries to update N entities in a loop", IsCorrect = false },
                    new QuestionOption { Text = "1 query for a list plus N separate queries for each item's related data, causing excessive database round-trips", IsCorrect = true },
                    new QuestionOption { Text = "A query that returns N+1 more rows than expected", IsCorrect = false },
                    new QuestionOption { Text = "An error when querying more than N related entities", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is the purpose of EF Core migrations?",
                Category = QuestionCategory.EfCore,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "EF Core migrations keep the database schema in sync with the entity model. Each migration captures a set of schema changes (add table, add column, etc.) that can be applied forward or rolled back, enabling version-controlled database evolution.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "To seed the database with initial data", IsCorrect = false },
                    new QuestionOption { Text = "To version-control and apply incremental schema changes to the database", IsCorrect = true },
                    new QuestionOption { Text = "To cache query results for better performance", IsCorrect = false },
                    new QuestionOption { Text = "To switch between different database providers", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is the difference between eager loading and lazy loading in EF Core?",
                Category = QuestionCategory.EfCore,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "Eager loading uses Include() to load related data in the same query. Lazy loading automatically loads related data the first time a navigation property is accessed (requires virtual navigation properties and a proxy package). Eager loading is generally preferred for known relationships.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Eager loading loads everything at startup; lazy loading loads on demand", IsCorrect = false },
                    new QuestionOption { Text = "Eager loading uses Include() to load related data in the initial query; lazy loading loads related data automatically on first access", IsCorrect = true },
                    new QuestionOption { Text = "They produce identical SQL — only the C# syntax differs", IsCorrect = false },
                    new QuestionOption { Text = "Lazy loading is always faster than eager loading", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "In EF Core, what happens when you call SaveChanges() without any tracked changes?",
                Category = QuestionCategory.EfCore,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "If there are no tracked changes in the ChangeTracker, SaveChanges() returns 0 without sending any SQL to the database. It is a safe no-op.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "It throws an InvalidOperationException", IsCorrect = false },
                    new QuestionOption { Text = "It performs a SELECT * on all tracked tables", IsCorrect = false },
                    new QuestionOption { Text = "It returns 0 and sends no SQL to the database", IsCorrect = true },
                    new QuestionOption { Text = "It commits an empty transaction", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is the purpose of a DbContext in EF Core?",
                Category = QuestionCategory.EfCore,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "DbContext is the primary class for interacting with EF Core. It represents a session with the database, tracking entity changes, managing transactions, and providing DbSet<T> properties for querying and saving entities.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "It defines the HTTP context for database requests", IsCorrect = false },
                    new QuestionOption { Text = "It is a unit of work representing a database session, managing entity tracking and change persistence", IsCorrect = true },
                    new QuestionOption { Text = "It generates SQL migrations automatically on every run", IsCorrect = false },
                    new QuestionOption { Text = "It is only used for configuring the connection string", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "How do you handle a database transaction explicitly in EF Core?",
                Category = QuestionCategory.EfCore,
                Difficulty = QuestionDifficulty.Hard,
                Explanation = "You use context.Database.BeginTransactionAsync() to start a transaction, then call CommitAsync() on success or RollbackAsync() on failure. Multiple SaveChanges() calls within the transaction are either all committed or all rolled back together.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Wrap all operations in a try-catch — EF Core handles transactions automatically", IsCorrect = false },
                    new QuestionOption { Text = "Use context.Database.BeginTransactionAsync(), then CommitAsync() or RollbackAsync()", IsCorrect = true },
                    new QuestionOption { Text = "Use the [Transaction] attribute on the DbContext class", IsCorrect = false },
                    new QuestionOption { Text = "Call context.SaveChanges() twice to create an implicit transaction", IsCorrect = false }
                }
            },

            // ── SQL Server ─────────────────────────────────────────────────────────

            new Question
            {
                Text = "What is the difference between a clustered index and a non-clustered index in SQL Server?",
                Category = QuestionCategory.SqlServer,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "A clustered index determines the physical storage order of rows in a table — there can only be one per table, typically on the primary key. A non-clustered index creates a separate structure pointing to the rows, and there can be many per table.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "A clustered index is stored in memory; a non-clustered index is on disk", IsCorrect = false },
                    new QuestionOption { Text = "A clustered index defines the physical row order; a non-clustered index creates a separate lookup structure", IsCorrect = true },
                    new QuestionOption { Text = "A non-clustered index is always faster than a clustered index", IsCorrect = false },
                    new QuestionOption { Text = "A table can have multiple clustered indexes", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is the difference between an INNER JOIN and a LEFT JOIN in SQL?",
                Category = QuestionCategory.SqlServer,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "An INNER JOIN returns only rows where there is a match in both tables. A LEFT JOIN returns all rows from the left table, plus matched rows from the right table. Rows in the left table with no match in the right table appear with NULL values for the right-side columns.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "INNER JOIN returns all rows; LEFT JOIN returns only unmatched rows", IsCorrect = false },
                    new QuestionOption { Text = "INNER JOIN returns only matched rows; LEFT JOIN returns all left rows plus matched right rows", IsCorrect = true },
                    new QuestionOption { Text = "LEFT JOIN is faster than INNER JOIN for large tables", IsCorrect = false },
                    new QuestionOption { Text = "They produce the same results when the tables have no NULLs", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What does the NOLOCK hint do in SQL Server and what risk does it introduce?",
                Category = QuestionCategory.SqlServer,
                Difficulty = QuestionDifficulty.Hard,
                Explanation = "NOLOCK (READ UNCOMMITTED) allows a query to read data without acquiring shared locks, improving read performance under high contention. However, it introduces the risk of dirty reads (reading uncommitted data that may be rolled back) and is generally discouraged for critical data.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "It prevents other sessions from reading the table while the query runs", IsCorrect = false },
                    new QuestionOption { Text = "It allows reading without shared locks, risking dirty reads of uncommitted data", IsCorrect = true },
                    new QuestionOption { Text = "It guarantees the most recent committed data is always returned", IsCorrect = false },
                    new QuestionOption { Text = "It disables all indexes for the duration of the query", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What SQL Server isolation level prevents dirty reads but still allows non-repeatable reads?",
                Category = QuestionCategory.SqlServer,
                Difficulty = QuestionDifficulty.Hard,
                Explanation = "READ COMMITTED is the SQL Server default. It prevents dirty reads by only reading committed data, but it does not prevent non-repeatable reads (a re-read of the same row can return different data if another transaction commits changes between reads).",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "READ UNCOMMITTED", IsCorrect = false },
                    new QuestionOption { Text = "READ COMMITTED", IsCorrect = true },
                    new QuestionOption { Text = "REPEATABLE READ", IsCorrect = false },
                    new QuestionOption { Text = "SERIALIZABLE", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is the purpose of a covering index in SQL Server?",
                Category = QuestionCategory.SqlServer,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "A covering index includes all the columns required by a query, allowing SQL Server to satisfy the query entirely from the index without doing a key lookup to the base table. This eliminates additional I/O and improves query performance.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "It covers all tables in a database with a single index", IsCorrect = false },
                    new QuestionOption { Text = "It includes all columns needed by a query, eliminating key lookups to the base table", IsCorrect = true },
                    new QuestionOption { Text = "It automatically generates indexes on all foreign key columns", IsCorrect = false },
                    new QuestionOption { Text = "It prevents other users from accessing a table while a query runs", IsCorrect = false }
                }
            },

            // ── LINQ ──────────────────────────────────────────────────────────────

            new Question
            {
                Text = "What is the key difference between IEnumerable<T> and IQueryable<T> in LINQ?",
                Category = QuestionCategory.Linq,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "IEnumerable<T> executes queries in memory (LINQ to Objects). IQueryable<T> builds an expression tree that is translated and executed by the provider (e.g., EF Core translates it to SQL). Using IQueryable keeps filtering at the database level; using IEnumerable can cause all data to be loaded first.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "IQueryable<T> can only be used with Entity Framework", IsCorrect = false },
                    new QuestionOption { Text = "IEnumerable<T> executes queries in memory; IQueryable<T> builds an expression tree executed by the provider (e.g., SQL)", IsCorrect = true },
                    new QuestionOption { Text = "IEnumerable<T> is faster than IQueryable<T> for database queries", IsCorrect = false },
                    new QuestionOption { Text = "There is no practical difference for most scenarios", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is deferred execution in LINQ, and when does the query actually run?",
                Category = QuestionCategory.Linq,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "Deferred execution means the LINQ query is not executed when defined — it is executed when you iterate over the result (e.g., foreach, ToList(), Count(), First()). This allows query composition without hitting the database until needed.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "The query runs when the variable is assigned", IsCorrect = false },
                    new QuestionOption { Text = "The query definition is stored and executed only when the result is iterated or a terminal operator is called", IsCorrect = true },
                    new QuestionOption { Text = "The query is always run on a background thread", IsCorrect = false },
                    new QuestionOption { Text = "Deferred execution only applies to in-memory collections", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "What is the difference between Select() and SelectMany() in LINQ?",
                Category = QuestionCategory.Linq,
                Difficulty = QuestionDifficulty.Medium,
                Explanation = "Select() projects each element of a sequence into a new form (one-to-one mapping). SelectMany() projects each element to a collection and flattens the results into a single sequence (one-to-many flattening). Use SelectMany() to 'unroll' nested collections.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Select() is for databases; SelectMany() is for in-memory collections", IsCorrect = false },
                    new QuestionOption { Text = "Select() maps one element to one result; SelectMany() maps one element to many and flattens the results", IsCorrect = true },
                    new QuestionOption { Text = "SelectMany() always performs better than Select()", IsCorrect = false },
                    new QuestionOption { Text = "They produce identical results for non-nested collections", IsCorrect = false }
                }
            },

            new Question
            {
                Text = "Why is using .Count() > 0 considered less efficient than .Any() in LINQ?",
                Category = QuestionCategory.Linq,
                Difficulty = QuestionDifficulty.Easy,
                Explanation = "Any() stops iterating as soon as it finds the first matching element. Count() must iterate the entire sequence to produce a total count, which is unnecessary if you only want to know whether any element exists. Against a database, Any() translates to EXISTS which is typically more efficient.",
                Options = new List<QuestionOption>
                {
                    new QuestionOption { Text = "Count() causes a full table scan; Any() uses an index", IsCorrect = false },
                    new QuestionOption { Text = "Any() stops at the first matching element; Count() iterates the entire sequence unnecessarily", IsCorrect = true },
                    new QuestionOption { Text = "They are equally efficient in all scenarios", IsCorrect = false },
                    new QuestionOption { Text = "Count() is always faster for in-memory collections", IsCorrect = false }
                }
            }
        };

        _context.Questions.AddRange(questions);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} interview questions.", questions.Count);
    }
}
