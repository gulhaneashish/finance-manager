CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Users" (
    "Id" int NOT NULL,
    "Name" nvarchar(max) NOT NULL,
    "Email" nvarchar(max) NOT NULL,
    "PasswordHash" nvarchar(max) NOT NULL,
    "CreatedAt" datetime2 NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260828063723_InitialCreate', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE "Accounts" (
    "Id" int NOT NULL,
    "UserId" int NOT NULL,
    "Name" nvarchar(max) NOT NULL,
    "Type" nvarchar(max) NOT NULL,
    "OpeningBalance" decimal(18,2) NOT NULL,
    "CreatedAt" datetime2 NOT NULL,
    CONSTRAINT "PK_Accounts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Accounts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Accounts_UserId" ON "Accounts" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260828091904_AddAccounts', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE "Categories" (
    "Id" int NOT NULL,
    "UserId" int NOT NULL,
    "Name" nvarchar(max) NOT NULL,
    "Type" nvarchar(max) NOT NULL,
    CONSTRAINT "PK_Categories" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Categories_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Transactions" (
    "Id" int NOT NULL,
    "UserId" int NOT NULL,
    "AccountId" int NOT NULL,
    "CategoryId" int,
    "Amount" decimal(18,2) NOT NULL,
    "Type" nvarchar(max) NOT NULL,
    "Description" nvarchar(max) NOT NULL,
    "TransactionDate" datetime2 NOT NULL,
    "CreatedAt" datetime2 NOT NULL,
    CONSTRAINT "PK_Transactions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Transactions_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id"),
    CONSTRAINT "FK_Transactions_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id"),
    CONSTRAINT "FK_Transactions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id")
);

CREATE INDEX "IX_Categories_UserId" ON "Categories" ("UserId");

CREATE INDEX "IX_Transactions_AccountId" ON "Transactions" ("AccountId");

CREATE INDEX "IX_Transactions_CategoryId" ON "Transactions" ("CategoryId");

CREATE INDEX "IX_Transactions_UserId" ON "Transactions" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260828094710_AddTransactionsAndCategories', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE "Budgets" (
    "Id" int NOT NULL,
    "UserId" int NOT NULL,
    "Year" int NOT NULL,
    "Month" int NOT NULL,
    "Income" decimal(18,2) NOT NULL,
    "ExpenseBudget" decimal(18,2) NOT NULL,
    "SavingsTarget" decimal(18,2) NOT NULL,
    "InvestmentTarget" decimal(18,2) NOT NULL,
    CONSTRAINT "PK_Budgets" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Budgets_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Budgets_UserId" ON "Budgets" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260828095620_AddBudgets', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "Transactions" DROP CONSTRAINT "FK_Transactions_Accounts_AccountId";

ALTER TABLE "Transactions" ALTER COLUMN "AccountId" DROP NOT NULL;

ALTER TABLE "Transactions" ADD "FromAccountId" int;

ALTER TABLE "Transactions" ADD "ToAccountId" int;

CREATE INDEX "IX_Transactions_FromAccountId" ON "Transactions" ("FromAccountId");

CREATE INDEX "IX_Transactions_ToAccountId" ON "Transactions" ("ToAccountId");

ALTER TABLE "Transactions" ADD CONSTRAINT "FK_Transactions_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Transactions" ADD CONSTRAINT "FK_Transactions_Accounts_FromAccountId" FOREIGN KEY ("FromAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT;

ALTER TABLE "Transactions" ADD CONSTRAINT "FK_Transactions_Accounts_ToAccountId" FOREIGN KEY ("ToAccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260831050138_AddTransferAccounts', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "Accounts" DROP COLUMN "CreatedAt";

ALTER TABLE "Accounts" RENAME COLUMN "Type" TO "AccountType";

ALTER TABLE "Accounts" ADD "IsActive" bit NOT NULL DEFAULT FALSE;

CREATE TABLE "Loans" (
    "Id" int NOT NULL,
    "UserId" int NOT NULL,
    "PersonName" nvarchar(max) NOT NULL,
    "Type" nvarchar(max) NOT NULL,
    "OriginalAmount" decimal(18,2) NOT NULL,
    "LoanDate" datetime2 NOT NULL,
    "DueDate" datetime2,
    "Notes" nvarchar(max),
    "IsActive" bit NOT NULL,
    CONSTRAINT "PK_Loans" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Loans_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "LoanPayments" (
    "Id" int NOT NULL,
    "LoanId" int NOT NULL,
    "AccountId" int NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    "PaymentDate" datetime2 NOT NULL,
    "Notes" nvarchar(max),
    CONSTRAINT "PK_LoanPayments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LoanPayments_Accounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_LoanPayments_Loans_LoanId" FOREIGN KEY ("LoanId") REFERENCES "Loans" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_LoanPayments_AccountId" ON "LoanPayments" ("AccountId");

CREATE INDEX "IX_LoanPayments_LoanId" ON "LoanPayments" ("LoanId");

CREATE INDEX "IX_Loans_UserId" ON "Loans" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260831060414_AddLoans', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "LoanPayments" ADD "TransactionId" int NOT NULL DEFAULT 0;

CREATE UNIQUE INDEX "IX_LoanPayments_TransactionId" ON "LoanPayments" ("TransactionId");

ALTER TABLE "LoanPayments" ADD CONSTRAINT "FK_LoanPayments_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES "Transactions" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260831061337_LinkLoanPaymentsToTransactions', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "Budgets" RENAME COLUMN "Income" TO "ExpectedIncome";

CREATE TABLE "CategoryBudgets" (
    "Id" int NOT NULL,
    "BudgetId" int NOT NULL,
    "CategoryId" int NOT NULL,
    "Amount" decimal(18,2) NOT NULL,
    CONSTRAINT "PK_CategoryBudgets" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CategoryBudgets_Budgets_BudgetId" FOREIGN KEY ("BudgetId") REFERENCES "Budgets" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CategoryBudgets_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_CategoryBudgets_BudgetId" ON "CategoryBudgets" ("BudgetId");

CREATE INDEX "IX_CategoryBudgets_CategoryId" ON "CategoryBudgets" ("CategoryId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260831064105_AddCategoryBudgets', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "Accounts" ADD "CreditLimit" decimal(18,2);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260831071351_AddCreditLimitToAccount', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "Transactions" ADD "Purpose" int NOT NULL DEFAULT 0;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260831072729_AddTransactionPurpose', '10.0.12');

COMMIT;

START TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260831105128_ConvertTransactionAndLoanTypesToEnums', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "Transactions" ALTER COLUMN "Purpose" TYPE nvarchar(max);
ALTER TABLE "Transactions" ALTER COLUMN "Purpose" SET DEFAULT 'Normal';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260901102509_FixTransactionType', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE "Investments" (
    "Id" int NOT NULL,
    "UserId" int NOT NULL,
    "Name" nvarchar(max) NOT NULL,
    "InvestmentType" int NOT NULL,
    "InvestedAmount" decimal(18,2) NOT NULL,
    "CurrentValue" decimal(18,2) NOT NULL,
    "InvestmentDate" datetime2 NOT NULL,
    "IsActive" bit NOT NULL,
    "CreatedAt" datetime2 NOT NULL,
    CONSTRAINT "PK_Investments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Investments_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Investments_UserId" ON "Investments" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260907093609_AddInvestment', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "Investments" ADD "TransactionId" int;

CREATE INDEX "IX_Investments_TransactionId" ON "Investments" ("TransactionId");

ALTER TABLE "Investments" ADD CONSTRAINT "FK_Investments_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES "Transactions" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260907095219_LinkInvestmentTransaction', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "Loans" ADD "AccountId" int NOT NULL DEFAULT 0;

ALTER TABLE "Loans" ADD "TransactionId" int;

CREATE INDEX "IX_Loans_TransactionId" ON "Loans" ("TransactionId");

ALTER TABLE "Loans" ADD CONSTRAINT "FK_Loans_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES "Transactions" ("Id");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260908072238_AddTransactionIdToLoan', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "Users" ADD "RefreshToken" nvarchar(max);

ALTER TABLE "Users" ADD "RefreshTokenExpiryTime" datetime2;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260930121839_AddRefreshTokenToUser', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "Users" ADD "IsActive" bit NOT NULL DEFAULT FALSE;

ALTER TABLE "Users" ADD "Role" nvarchar(max) NOT NULL DEFAULT '';

CREATE TABLE "AuditLogs" (
    "Id" int NOT NULL,
    "UserId" int,
    "UserEmail" nvarchar(max) NOT NULL,
    "Action" nvarchar(max) NOT NULL,
    "Details" nvarchar(max) NOT NULL,
    "Timestamp" datetime2 NOT NULL,
    CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261002054012_AddAdminRoleAndAuditLogs', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE "Users" ADD "LastLoginAt" datetime2;

ALTER TABLE "Users" ADD "PhoneNumber" nvarchar(max);

ALTER TABLE "Users" ADD "ProfilePictureUrl" nvarchar(max);

ALTER TABLE "Users" ADD "Username" nvarchar(max);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261002063101_AddUserProfileFields', '10.0.12');

COMMIT;

