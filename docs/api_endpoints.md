# API Endpoint List

This document provides a comprehensive, reverse-engineered audit of the C# .NET 8 Web API endpoints for the Subscription & Automatic Payment Reminder Application. The endpoints have been thoroughly analyzed to reflect the current state of the codebase, including business logic constraints, database invariants, and domain-driven design decisions.

## RESTful API Endpoints Matrix

| HTTP Method | Endpoint URI | Business Context | Request Payload | Success Response | Validation & Errors |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **POST** | `/api/customers` | Creates a new customer profile. Adheres to domain immutability constraints. | `CreateCustomerDto` | `CustomerDto` <br/> *201 Created* | `400 Bad Request` (Validation errors) |
| **GET** | `/api/customers` | Retrieves all active customers in the system, excluding soft-deleted records. | None | `List<CustomerDto>` <br/> *200 OK* | None |
| **GET** | `/api/customers/{id}` | Fetches a specific customer by their unique identifier. | None | `CustomerDto` <br/> *200 OK* | `404 Not Found` |
| **DELETE** | `/api/customers/{id}` | Performs a soft delete on a customer profile. | None | *204 No Content* | `404 Not Found` <br/> `409 Conflict` (Fails due to restricted navigation if dependent financial logs/subscriptions exist) |
| **POST** | `/api/subscriptions` | Creates a new subscription linked to an existing customer. | `CreateSubscriptionDto` | `SubscriptionDto` <br/> *201 Created* | `400 Bad Request` <br/> `404 Not Found` (Customer not found) |
| **GET** | `/api/subscriptions` | Retrieves all active subscriptions, filtered by soft delete constraints. | None | `List<SubscriptionDto>` <br/> *200 OK* | None |
| **GET** | `/api/subscriptions/{id}` | Fetches details of a specific subscription. | None | `SubscriptionDto` <br/> *200 OK* | `404 Not Found` |
| **GET** | `/api/subscriptions/customer/{customerId}` | Retrieves all active subscriptions associated with a specific customer. | None | `List<SubscriptionDto>` <br/> *200 OK* | `404 Not Found` |
| **PUT** | `/api/subscriptions/{id}` | Updates existing subscription parameters (e.g., amount, due date). | `UpdateSubscriptionDto` | `SubscriptionDto` <br/> *200 OK* | `400 Bad Request` (ID mismatch) <br/> `404 Not Found` |
| **DELETE** | `/api/subscriptions/{id}` | Soft deletes a subscription. | None | *204 No Content* | `404 Not Found` <br/> `409 Conflict` (If dependent payment history prevents deletion) |
| **POST** | `/api/payments/process` | Processes a subscription payment synchronously, ensuring financial idempotency. | `ProcessPaymentDto` | `PaymentResultDto` <br/> *200 OK* | `400 Bad Request` <br/> `404 Not Found` <br/> `409 Conflict` (Triggered by Composite Unique Index breach preventing duplicate payments for the same period) |
| **GET** | `/api/payments/subscription/{subscriptionId}` | Retrieves the complete payment history for a specific subscription. | None | `List<PaymentHistoryDto>` <br/> *200 OK* | None |
| **GET** | `/api/reminders/pending` | Identifies unpaid subscriptions approaching their due dates. Utilizes `Task.WhenAll` for high-speed concurrent external debt checking. | `[Query] thresholdDays` (int, default: 5) | `List<ReminderNotificationDto>` <br/> *200 OK* | None |
| **GET** | `/api/summary/active-subscriptions` | Retrieves a flattened summary of all currently active subscriptions. | None | `List<SubscriptionDto>` <br/> *200 OK* | None |
| **GET** | `/api/summary/unpaid-subscriptions` | Fetches a summary list of subscriptions that have not yet been paid for the current billing period. | None | `List<UnpaidSubscriptionDto>` <br/> *200 OK* | None |
| **GET** | `/api/summary/payment-history` | Provides a comprehensive system-wide summary of all processed payment logs. | None | `List<SubscriptionPaymentSummaryDto>` <br/> *200 OK* | None |

---

### 🛡️ Core Architectural Rules & Constraints

1. **Customer Immutability:** Notice that **`PUT/PATCH /api/customers` does not exist**. This is an intentional architectural design reflecting strict domain immutability rules. Once a customer is created, their core domain entity cannot be haphazardly mutated.
2. **Soft Delete Integrity:** For `DELETE` operations on core entities (e.g., `DELETE /api/customers/{id}` and `DELETE /api/subscriptions/{id}`), the system employs soft deletes via EF Core Query Filters. These operations will fail with a **`409 Conflict`** (restricted navigation behavior) if attempting to delete records that have dependent financial logs (Payments), ensuring strict relational data integrity.
3. **Financial Idempotency:** The `POST /api/payments/process` endpoint acts as a transaction guard. It explicitly returns a **`409 Conflict`** if a duplicate payment is attempted for the same subscription and period. This is enforced at the database level by a **Composite Unique Index**, guaranteeing financial idempotency and zero-duplicate billing.
4. **Optimized Concurrency:** The `GET /api/reminders/pending` endpoint is heavily optimized for I/O operations. It fetches external debt data by executing pre-allocated Polly policies and utilizes **`Task.WhenAll`** to process external debt-checking network requests concurrently. This achieves high-speed debt evaluation with partial failure isolation.
