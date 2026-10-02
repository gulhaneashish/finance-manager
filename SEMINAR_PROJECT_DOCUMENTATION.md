# Personal Finance Manager — Complete Technical Seminar & Viva Guide

---

## 1. Executive Summary & Project Overview

### What is the Personal Finance Manager?
The **Personal Finance Manager** is an enterprise-grade full-stack financial tracking and budgeting web application. It empowers individuals to manage multi-source personal finances with accuracy, security, and real-time analytical insights.

### Core Business Capabilities:
1. **Multi-Account Management**: Supports Bank Accounts, Cash, Savings, and Credit Cards with distinct financial behaviors.
2. **Double-Entry Style Fund Transfers**: Secure balance transfers between accounts with atomic transaction rollback.
3. **Credit Card Accounting**: Tracks credit limit, dynamic outstanding balances, purchases, and bill settlements.
4. **Budgeting Engine**: Monthly envelope budgeting with category-wise spending limits and variance tracking.
5. **Loan Management**: Tracks both borrowed (liabilities) and lent (assets) loans, due dates, and progressive payment installments.
6. **Investment Portfolio**: Tracks investments (Stocks, Mutual Funds, Fixed Deposits, Gold, Crypto), real-time valuation, and profit/loss calculations.
7. **Financial Analytics & Reports**: Live net worth calculation, monthly cash flow, category expense distributions, and debt summaries.

---

## 2. System Architecture & Request Lifecycle

The backend is built following the **Clean 3-Tier Architecture (N-Tier Pattern)** to guarantee Separation of Concerns (SoC), testability, maintainability, and loose coupling.

### Architecture Diagram

```
[ Client: Angular 21 (HTTP / JSON) ]
              │
              ▼  (JWT Bearer Auth)
┌──────────────────────────────────────────────────────────┐
│                   CONTROLLER LAYER                       │
│  - Receives HTTP Requests (GET, POST, PUT, DELETE)       │
│  - Extracts User Identity from Claims (ClaimsPrincipal)  │
│  - Validates DTO Models & Returns HTTP Status Codes     │
│  - Depends ONLY on Service Interfaces (Loose Coupling)   │
└─────────────────────────────┬────────────────────────────┘
                              │
                              ▼  (Invokes IService methods)
┌──────────────────────────────────────────────────────────┐
│                    SERVICE LAYER                         │
│  - Contains ALL Domain Business Logic & Validations      │
│  - Manages Business Calculations (Balances, Net Worth)  │
│  - Coordinates ACID Transactions across Repositories     │
│  - Returns clean DTOs (Data Transfer Objects)            │
│  - Implements Service Interfaces (e.g., IAccountService) │
└─────────────────────────────┬────────────────────────────┘
                              │
                              ▼  (Invokes IRepository methods)
┌──────────────────────────────────────────────────────────┐
│                   REPOSITORY LAYER                       │
│  - Abstract Data Access Layer (IRepository<T>)           │
│  - Encapsulates EF Core queries, filtering & joins       │
│  - Generic Repository base + Entity-Specific Repositories│
│  - Exposes Query(), AddAsync(), Remove(), SaveChanges()  │
└─────────────────────────────┬────────────────────────────┘
                              │
                              ▼  (Translates LINQ to SQL)
┌──────────────────────────────────────────────────────────┐
│             DATA ACCESS / ORM (EF Core)                  │
│  - FinanceDbContext & DbSet<T>                           │
│  - ChangeTracker, Fluent API entity configurations       │
└─────────────────────────────┬────────────────────────────┘
                              │
                              ▼  (T-SQL queries)
┌──────────────────────────────────────────────────────────┐
│                DATABASE: SQL SERVER                      │
│  - ACID-compliant relational storage                     │
│  - Foreign Keys, Constraints, Cascades, Indexes          │
└──────────────────────────────────────────────────────────┘
```

---

## 3. Technology Stack & "Why This and Why Not That?"
*(This is the most critical section for your seminar panel/examiners.)*

### Q1: Why ASP.NET Core Web API (.NET 10) instead of Node.js / Express or Python Django?
* **High Performance**: ASP.NET Core is ranked among the fastest web frameworks in the TechEmpower benchmarks due to the Kestrel web server, compiled C# bytecode, and zero-allocation socket optimizations.
* **Strong Typing & Compile-Time Safety**: Financial applications cannot afford runtime `undefined is not a function` or silent type-coercion bugs common in JavaScript. C#'s type safety guarantees correctness before deployment.
* **Built-in Dependency Injection**: Inversion of Control (IoC) is a first-class citizen in ASP.NET Core, unlike Express where external libraries or custom boilerplate are required.
* **Security & Compliance**: Built-in enterprise authentication, cryptographic helpers, and RFC-standardized error formatting.

### Q2: Why Entity Framework Core instead of Raw ADO.NET or Dapper?
* **Productivity & Maintainability**: EF Core eliminates repetitive boilerplate SQL strings, data reader mapping, and manual connection state management.
* **LINQ (Language Integrated Query)**: Strongly typed queries validated at compile-time. If a table column changes, compile errors alert the developer immediately, preventing runtime SQL syntax crashes.
* **Change Tracker**: Automatically detects modified entity properties and issues optimal SQL update statements.
* **Database Migrations**: Version-controlled schema evolutions allow the database schema to stay in sync with C# domain models across environments.
* **When would Dapper be used?** For extreme high-throughput read reporting where every microsecond matters. But for business domain logic, EF Core provides superior maintainability and safety.

### Q3: Why SQL Server (Relational) instead of MongoDB (NoSQL)?
* **ACID Transactions**: Financial operations (such as transferring money from Account A to Account B) require absolute Atomicity, Consistency, Isolation, and Durability. If money leaves Account A, it *must* arrive in Account B, or both operations must abort. NoSQL eventual consistency is unacceptable for banking and balance computations.
* **Referential Integrity**: Foreign keys ensure transactions cannot reference non-existent accounts or users.
* **Complex Aggregations & Joins**: Financial analytics (grouping expenses by category, calculating monthly cash flows, net worth) are natural, indexed relational queries.

### Q4: Why the Repository Pattern + Service Interfaces instead of injecting DbContext directly into Controllers?
* **Testability (Mocking)**: In unit tests, we can easily mock `IAccountRepository` or `IAccountService` using Moq without needing a real database connection.
* **Separation of Concerns (SoC)**: Controllers handle HTTP concerns (status codes, routes, query params). Services handle business calculations and rules. Repositories handle database interaction.
* **Avoid Database Leaks**: Injecting `DbContext` directly into Controllers exposes database queries and entity states to the presentation layer, violating architectural boundaries.
* **Centralized Data Queries**: If query optimization or caching is needed in the future, it is updated in one Repository method rather than scattered across dozens of controllers.

### Q5: Why Service Interfaces (`IAccountService`, etc.)?
* **Dependency Inversion Principle (SOLID)**: High-level modules (Controllers) should not depend on low-level concrete modules (Services); both should depend on abstractions (Interfaces).
* **Interchangeability**: The implementation can be swapped (e.g., swapping a MockService or upgraded Service) without changing a single line of Controller code.

### Q6: Why JWT (JSON Web Tokens) instead of Server-Side Session Cookies?
* **Statelessness & Scalability**: The server does not store user session states in memory or Redis. Every request carries its own cryptographic signature, allowing horizontal scaling across multiple servers without sticky sessions.
* **Cross-Origin & Mobile-Ready**: JWT tokens can be consumed by Angular, mobile apps (Flutter, React Native), and third-party integrations with equal ease via standard `Authorization: Bearer <token>` headers.
* **Embedded Claims**: User ID, email, and roles are embedded inside the token payload, allowing controllers to extract identity instantly without querying the database on every route.

### Q7: Why BCrypt instead of SHA-256 or MD5 for Password Storage?
* **MD5 and plain SHA-256 are NOT safe for passwords**: Fast hashing algorithms allow hackers to compute billions of hashes per second using modern GPUs and precomputed rainbow tables.
* **BCrypt is Slow by Design**: BCrypt incorporates an adjustable work factor (cost factor) and automatic cryptographic salting. This makes brute-force and dictionary attacks computationally infeasible.

### Q8: Why Scoped Service Lifetime in `Program.cs` (`AddScoped`) instead of Singleton or Transient?
* **DbContext Thread Safety**: EF Core's `DbContext` is **not thread-safe**. If registered as `Singleton`, concurrent HTTP requests would share the same context instance, causing race conditions, corrupted ChangeTracker states, and crashes.
* **Why not Transient?** If `Transient` is used, multiple services invoked within the same HTTP request would each receive a different `DbContext` instance, making shared database transactions and unit of work impossible.
* **Scoped is the Gold Standard for Web Requests**: A single `DbContext`, Repository, and Service instance is instantiated per incoming HTTP request and cleanly disposed of when the HTTP response completes.

---

## 4. Deep-Dive Code Walkthrough

### 4.1 The Repository Layer
All repositories inherit from the generic base:

```csharp
// Repositories/Interfaces/IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    IQueryable<T> Query();
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
```

* **`IQueryable<T> Query()`**: Allows services to compose customized async projections (`.Select()`), aggregations (`.SumAsync()`, `.CountAsync()`), and eager includes (`.Include()`) while keeping query execution delayed until the database roundtrip.
* **`BeginTransactionAsync()`**: Enables multiple repository operations to participate in an explicit database transaction managed by the business layer.

### 4.2 The Service Layer
Services encapsulate business rules:
- Validate domain invariants (e.g., negative balance checks, date boundaries, status conditions).
- Ensure credit card limits are not exceeded before authorizing purchases.
- Execute calculations and construct clean Response DTOs.

Example: [`AccountBalanceService.cs`](file:///d:/.net%20learning/finance-manager/backend/FinanceManager.API/FinanceManager.API/Services/AccountBalanceService.cs)
```csharp
public class AccountBalanceService : IAccountBalanceService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    ...
```
* **Dynamic Balance Calculation**: Balances are calculated dynamically by aggregating initial opening balance + income + deposits + transfers in − expenses − transfers out − credit card settlements. This guarantees balance integrity even if historical records are reviewed.

### 4.3 The Controller Layer
Controllers are clean and slim:
- Authorize requests via `[Authorize]`.
- Extract the authenticated user's ID securely from the JWT token via `User.FindFirstValue(ClaimTypes.NameIdentifier)`.
- Invoke the injected `IServiceInterface` and return standard HTTP status codes (`200 OK`, `201 Created`, `204 NoContent`, `400 BadRequest`, `401 Unauthorized`, `404 NotFound`).

### 4.4 Global Exception Handling & RFC 7807 ProblemDetails
Located in [`GlobalExceptionHandler.cs`](file:///d:/.net%20learning/finance-manager/backend/FinanceManager.API/FinanceManager.API/Exceptions/GlobalExceptionHandler.cs):
- Catches unhandled exceptions centrally using ASP.NET Core's `IExceptionHandler`.
- Maps specific domain exceptions:
  - `InvalidOperationException` ➔ `400 Bad Request`
  - `KeyNotFoundException` ➔ `404 Not Found`
  - Unhandled exceptions ➔ `500 Internal Server Error`
- Returns consistent, standardized JSON payloads conforming to **RFC 7807 (ProblemDetails)**.

---

## 5. End-to-End Request Flows

### Scenario: Fund Transfer Between Accounts
```
1. Client sends POST /api/Transaction/transfer with { fromAccountId, toAccountId, amount: 5000, purpose: "Savings" }
2. TransactionController receives request, validates user JWT, passes DTO and userId to ITransactionService.
3. TransactionService checks:
   - Does fromAccount belong to user and is active?
   - Does toAccount belong to user and is active?
   - Is fromAccount balance >= transfer amount?
   - Credit card restrictions (cannot transfer directly from credit card).
4. TransactionService begins database transaction:
   await using var transaction = await _transactionRepository.BeginTransactionAsync();
5. Transaction record created with Type: Transfer, Purpose: Savings.
6. _transactionRepository.AddAsync(transfer) + SaveChangesAsync().
7. Transaction committed: await transaction.CommitAsync().
   (If any error occurs, transaction.RollbackAsync() triggers automatically in catch block).
8. Controller returns 200 OK.
```

---

## 6. Seminar / Viva Q&A Cheat Sheet (Top 20 Questions)

#### Q1: What design patterns did you use in this project?
> **Answer**: 
> 1. **Repository Pattern**: Decouples business logic from EF Core data access.
> 2. **Dependency Injection Pattern**: Built-in IoC container manages lifetimes.
> 3. **Data Transfer Object (DTO) Pattern**: Isolates internal database entity models from API contracts.
> 4. **Middleware Pattern**: For global exception handling and JWT authentication.

#### Q2: What is the difference between `AddScoped`, `AddTransient`, and `AddSingleton`?
> **Answer**:
> - `Transient`: A new instance is created every time it is requested.
> - `Scoped`: A single instance is created per HTTP request lifecycle and shared across all consumers in that request.
> - `Singleton`: A single instance is created once on app startup and shared across all requests and all users.

#### Q3: How do you prevent SQL Injection in this project?
> **Answer**: EF Core uses **parameterized queries** under the hood for all LINQ expressions. User input values are passed as SQL parameters, never concatenated directly into raw SQL strings.

#### Q4: How is password security handled?
> **Answer**: Passwords are never stored in plain text. We use the **BCrypt.Net** hashing algorithm with automatic cryptographic salting and an adaptive cost factor.

#### Q5: What is the purpose of DTOs? Why not return database models directly?
> **Answer**:
> 1. **Security (Prevent Over-posting)**: Protects sensitive columns (e.g., `PasswordHash`) from being exposed.
> 2. **Avoid Circular Reference Errors**: EF Core models have reciprocal navigation properties (`User.Transactions` and `Transaction.User`), which cause JSON serialization loops.
> 3. **Contract Stability**: Database schema can evolve without breaking client frontend contracts.

#### Q6: How does the application handle database transactions?
> **Answer**: In multi-step operations (e.g., transfers, loan creations, investment sales), we use explicit EF Core execution transactions: `await using var dbTransaction = await _repository.BeginTransactionAsync()`. If any step fails, `await dbTransaction.RollbackAsync()` guarantees database consistency.

#### Q7: How does JWT authentication work in your application?
> **Answer**: 
> 1. User logs in via `POST /api/User/login` with email and password.
> 2. If valid, server generates a signed JWT using a HMAC-SHA256 secret key with claims (`NameIdentifier`, `Name`, `Email`).
> 3. Client stores token and sends it in the `Authorization: Bearer <token>` header for subsequent requests.
> 4. ASP.NET Core `JwtBearer` middleware validates the signature, issuer, audience, and expiration before reaching controllers.

#### Q8: What is CORS and why did you configure it?
> **Answer**: Cross-Origin Resource Sharing (CORS) is a browser security mechanism blocking web pages from requesting a domain different from the one that served the page. Since our Angular frontend runs on `http://localhost:4200` and the backend runs on `http://localhost:5228`, we configured a CORS policy allowing `http://localhost:4200` with headers and methods.

#### Q9: How does the Credit Card module calculate outstanding balances?
> **Answer**: Outstanding balance is dynamically computed as:
> `Total Purchases (Transactions where Purpose == CreditCardPurchase) − Total Payments (Transactions where Purpose == CreditCardPayment)`.
> Available Credit is then computed as `CreditLimit − OutstandingBalance`.

#### Q10: How does Net Worth calculation work?
> **Answer**:
> `Net Worth = Total Assets − Total Liabilities`.
> - **Assets**: Bank Balances + Cash + Savings + Current Investment Values + Loans Lent (Money owed to the user).
> - **Liabilities**: Credit Card Outstanding Debt + Loans Borrowed (Money the user owes to others).

#### Q11: What is ProblemDetails?
> **Answer**: RFC 7807 standard specification for HTTP APIs to return machine-readable details of errors in a uniform JSON format including `status`, `title`, `detail`, and `instance`.

#### Q12: Why did you use `decimal` data type instead of `double` or `float` for money?
> **Answer**: `float` and `double` are binary floating-point types that cannot represent base-10 fractions accurately, leading to rounding errors (e.g., `0.1 + 0.2 = 0.30000000000000004`). `decimal` provides 128-bit base-10 precision with 28-29 significant digits, making it the mandatory standard for currency and financial calculations.

#### Q13: What happens when an invalid endpoint or unhandled exception occurs?
> **Answer**: It is intercepted by our custom `GlobalExceptionHandler` which logs the error using `ILogger` and returns a standardized `ProblemDetails` response with proper HTTP status code without exposing stack traces to clients in production.

#### Q14: How are Soft Deletes implemented for accounts?
> **Answer**: Rather than physically deleting the account row (which would cause foreign key violation cascades on historical transactions), we toggle `account.IsActive = false`. All active queries filter by `IsActive == true`.

#### Q15: What is Swagger / OpenAPI in your project?
> **Answer**: Swagger (OpenAPI 3.0) dynamically inspects API controllers, models, and attributes, generating an interactive web interface at `/swagger` for live endpoint documentation, schema definitions, and testing with JWT token authentication.

#### Q16: What is a Refresh Token, why did you add it, and how does it work?
> **Answer**:
> - **Why not just a long-lived JWT?**: Access tokens (JWTs) are stateless and cannot be revoked without maintaining a blacklist. If an access token with a 30-day validity is intercepted by an attacker, they have unrestricted access for 30 days.
> - **Dual-Token Pattern**:
>   1. **Short-Lived Access Token (JWT)**: Valid for minutes (15–60 min) with user claims, minimizes exposure window if leaked.
>   2. **Long-Lived Refresh Token**: A cryptographically random 64-byte token stored in the database (`Users.RefreshToken` and `RefreshTokenExpiryTime`, 7 days).
> - **Refresh Token Rotation**: When `/api/User/refresh-token` is called, our `AuthService` validates the token, invalidates the old refresh token, and generates a brand-new access token AND a new refresh token. Replaying an old token results in an immediate `401 Unauthorized` (Anti-Replay protection).
> - **Revocation / Logout**: On logout, `/api/User/revoke-token` clears the refresh token from the database, instantly terminating the session.
> - **Angular Silent Refresh**: The Angular `authInterceptor` catches `401 Unauthorized` errors, calls `authService.refreshToken()`, saves the new tokens, and transparently replays the user's failed request with the new access token.

---

## 7. Tips for Your Seminar Presentation

1. **Start with the Problem**: "Managing personal finances across multiple bank accounts, cash, credit cards, loans, and investments often leads to fragmented records and inaccurate balances."
2. **Present the Solution**: "We built an enterprise-style Personal Finance Manager using .NET 10, EF Core, and Angular, adhering to Clean Architecture principles."
3. **Emphasize Architecture**: Show the 3-Tier diagram. Mention: *"Controllers only handle routing and presentation, Services enforce business rules, and Repositories isolate database access."*
4. **Highlight Financial Safety**: Mention ACID transactions, `decimal` precision, and dynamic balance calculations.
5. **Demonstrate Confidence**: When asked *"Why didn't you use X?"*, refer back to Section 3 of this guide!
