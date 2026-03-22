**Equipment Rental System**

A cross-platform inventory and rental management application built with .NET MAUI and PostgreSQL.
The system allows businesses to manage equipment inventory, customers, and rental transactions through a unified interface that works across desktop and mobile platforms.

The application replaces manual inventory tracking with a centralized digital solution that ensures accurate asset availability, automated pricing, and reliable transaction management.

**Features**

- Cross-platform support for Windows, Android, and iOS

- Real-time equipment inventory tracking

- Customer management system

- Automated rental cost calculations

- Equipment availability tracking

- Transaction-safe rental operations

- Full CRUD operations for all entities

**Tech Stack**

Frontend
- .NET MAUI
- XAML
- CSS

Backend
- C#

Database
- PostgreSQL
- Npgsql (.NET PostgreSQL driver)

Architecture
- MVVM
  
**Database Design**

The relational database manages the following core entities:
- Customer  | Stores customer information and banned status
- Equipment |	Tracks inventory items and availability
- Category  | Groups equipment into logical categories
- Rental    | Records rental transactions

Relationships ensure data integrity between customers, equipment, and rental records.

**Technical Highlights**

Asynchronous Database Operations

All database interactions use C# async/await patterns to prevent UI blocking and improve performance.

Transactional Rental Processing

Rental creation uses database transactions to guarantee atomic updates:

1. Insert rental record

2. Update equipment availability

3. Commit both operations together

If an error occurs, the transaction is rolled back to prevent inconsistent inventory states.

Parameterized SQL Queries

**Environment Configuration**

Database credentials are loaded from environment variables to prevent sensitive data from being stored in source code.

Required variables:
- DB_HOST
- DB_USER
- DB_PASSWORD
- DB_NAME

Example local setup:
- DB_HOST=localhost
- DB_USER=postgres
- DB_PASSWORD=yourpassword
- DB_NAME=rental_db


**Running the Project**
1. Clone the repository
- git clone https://github.com/Lucas-Urbanski/EquipmentRentalSystem

2. Configure PostgreSQL
- CREATE DATABASE rental_db.
- scripts included in /Data.

3. Run the application
- dotnet build
- dotnet run

**Engineering Concepts Demonstrated**
- Cross-platform application development
- MVVM architectural design
- Asynchronous database programming
- Transaction-safe database operations
- Relational database modeling
- Secure configuration management

**Author**

Lucas Urbanski

Software Developer
