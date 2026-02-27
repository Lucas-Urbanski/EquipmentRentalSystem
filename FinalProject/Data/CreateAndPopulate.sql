DROP TABLE IF EXISTS RENTAL;
DROP TABLE IF EXISTS CUSTOMER;
DROP TABLE IF EXISTS EQUIPMENT;
DROP TABLE IF EXISTS CATEGORY;

-- Create the CATEGORY table
CREATE TABLE CATEGORY (
    categoryID SERIAL PRIMARY KEY,
    type VARCHAR(50) NOT NULL
);

-- Create the EQUIPMENT table
CREATE TABLE EQUIPMENT (
    equipmentID SERIAL PRIMARY KEY,
    categoryID INTEGER NOT NULL,
    name VARCHAR(50) NOT NULL,
    description VARCHAR(200) NOT NULL,
    daily_rate DECIMAL NOT NULL,
    status BOOLEAN,
    FOREIGN KEY (categoryID) REFERENCES CATEGORY(categoryID)
);

-- Create the CUSTOMER table
CREATE TABLE CUSTOMER (
    customerID SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    phone_number VARCHAR(12),
    email VARCHAR(50) NOT NULL CHECK (email LIKE '%@%.%'),
    banned BOOLEAN NOT NULL DEFAULT FALSE
);

-- Create the RENTAL table
CREATE TABLE RENTAL (
    rentalID SERIAL PRIMARY KEY,
    customerID INTEGER NOT NULL,
    equipmentID INTEGER NOT NULL,
    date TIMESTAMP,
    rental_date TIMESTAMP NOT NULL,
    return_date TIMESTAMP NOT NULL,
    total_cost DECIMAL NOT NULL,
    FOREIGN KEY (customerID) REFERENCES CUSTOMER(customerID),
    FOREIGN KEY (equipmentID) REFERENCES EQUIPMENT(equipmentID)
);

-- CATEGORY data
INSERT INTO CATEGORY (categoryID, type)
VALUES 
    (10, 'Power Tools'),
    (20, 'Yard Equipment'),
    (30, 'Compressors'),
    (40, 'Generators'),
    (50, 'Air Tools');

-- EQUIPMENT data
INSERT INTO EQUIPMENT (
    equipmentID, categoryID, name, description, daily_rate, status
)
VALUES
    (101, 10, 'Hammer Drill', 'Powerful drill for concrete and masonry', 25.99, TRUE),
    (201, 20, 'Chainsaw', 'Gas-powered chainsaw for cutting wood', 49.99, TRUE),
    (202, 20, 'Lawn Mower', 'Self-propelled lawn mower with mulching function', 19.99, TRUE),
    (301, 30, 'Small Compressor', '5 Gallon Compressor-Portable', 14.99, TRUE),
    (501, 50, 'Brad Nailer', 'Brad Nailer. Requires 3/4 to 1 1/2 Brad Nails', 10.99, TRUE);

-- CUSTOMER data
INSERT INTO CUSTOMER (
    customerID, first_name, last_name, phone_number, email, banned
)
VALUES
    (1001, 'John',  'Doe',  '403-111-2222', 'John@outlook.ca', FALSE),
    (1002, 'Jane',  'Smith','587-333-4444', 'Jane@gmail.ca', FALSE),
    (1003, 'Peter', 'Jones','403-555-6666', 'Peter@yahoo.ca', FALSE);

-- RENTAL data
INSERT INTO RENTAL (
    rentalID, customerID, equipmentID, date, rental_date, return_date, total_cost
)
VALUES
    (10001, 1001, 201, '2025-04-20', '2025-04-15', '2025-04-29', 149.97),
    (10002, 1002, 501, '2025-02-23', '2025-02-21', '2025-02-28', 43.96);