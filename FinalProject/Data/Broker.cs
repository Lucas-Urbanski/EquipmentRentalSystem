using Npgsql; // Imports the Npgsql library for PostgreSQL database interaction.

namespace FinalProject.Data
{
    // A class responsible for handling database operations.
    public class Broker
    {
        // Field to store the database connection string.
        private readonly string _connectionString; 

        // Constructor: Initializes the connection string.
        public Broker()
        {
            // Loads environment variables from a .env file if present.
            DotNetEnv.Env.Load(); 

            var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
            var pass = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var db = Environment.GetEnvironmentVariable("DB_NAME") ?? "rental_db";
            
            // Uses NpgsqlConnectionStringBuilder to construct the connection string securely.
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = 5432,
                Username = user,
                Password = pass,
                Database = db,
                Pooling = true
            };

            // Stores the constructed connection string.
            _connectionString = builder.ConnectionString;
        }

        // Asynchronously gets and opens a new NpgsqlConnection.
        public async Task<NpgsqlConnection> GetOpenConnectionAsync()
        {
            // Creates a new NpgsqlConnection using the stored connection string.
            var conn = new NpgsqlConnection(_connectionString);
            try
            {
                // Attempts to asynchronously open the database connection.
                await conn.OpenAsync();
            }
            catch (Exception ex)
            {
                // Writes a warning to the console if the connection fails to open.
                Console.WriteLine($"WARNING: Could not open DB connection: {ex.Message}");
            }
            // Returns the connection object (it might be closed if an exception occurred).
            return conn;
        }

        // ----------------------
        // CUSTOMER
        // ----------------------

        // Asynchronously inserts a new Customer record into the database.
        public async Task<int> InsertCustomerAsync(Customer c)
        {
            // SQL query to insert a customer and return the newly generated customerID.
            const string sql = @"
                INSERT INTO customer (first_name, last_name, phone_number, email)
                VALUES (@first, @last, @phone, @email)
                RETURNING customerID;
            ";

            // Creates and opens the connection, ensuring it's disposed with 'await using var'.
            await using var conn = await GetOpenConnectionAsync();
            // Checks if the connection was successfully opened; returns -1 if not.
            if (conn.State != System.Data.ConnectionState.Open) return -1;
            // Creates a new command with the SQL and connection, ensuring disposal.
            await using var cmd = new NpgsqlCommand(sql, conn);
            // Adds parameters to the command. Uses DBNull.Value for null C# properties.
            cmd.Parameters.AddWithValue("first", c.Firstname ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("last", c.Lastname ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("phone", c.PhoneNumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("email", c.Email ?? (object)DBNull.Value);
            // Executes the query and retrieves the first column of the first row (the customerID).
            var obj = await cmd.ExecuteScalarAsync();
            // Returns the new ID if successful, otherwise returns -1.
            return obj is not null ? Convert.ToInt32(obj) : -1;
        }

        // Asynchronously loads all Customer records from the database.
        public async Task<List<Customer>> LoadAllCustomersAsync()
        {
            // Initializes an empty list to store the customers.
            var list = new List<Customer>();
            // SQL query to select all columns from the customer table, ordered by ID.
            const string sql = @"SELECT customerID, first_name, last_name, phone_number, email, banned FROM customer ORDER BY customerID;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command.
            await using var cmd = new NpgsqlCommand(sql, conn);
            // Executes the command and gets a data reader, ensuring disposal.
            await using var reader = await cmd.ExecuteReaderAsync();
            // Reads records one by one.
            while (await reader.ReadAsync())
            {
                // Creates a new Customer object and maps the column values to its properties.
                list.Add(new Customer
                {
                    CustomerID = reader.GetInt32(0),
                    // Checks for DBNull before reading string values.
                    Firstname = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Lastname = reader.IsDBNull(2) ? null : reader.GetString(2),
                    PhoneNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                    // Handles the 'banned' boolean value, treating DBNull as false.
                    Banned = !reader.IsDBNull(5) && reader.GetBoolean(5)
                });
            }
            // Returns the list of customers.
            return list;
        }

        // Asynchronously loads a single Customer record by its ID.
        public async Task<Customer?> LoadCustomerByIdAsync(int id)
        {
            // SQL query to select a specific customer by their ID.
            const string sql = @"SELECT customerID, first_name, last_name, phone_number, email FROM customer WHERE customerID = @id;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command and adds the ID parameter.
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            // Executes the command and gets a data reader.
            await using var reader = await cmd.ExecuteReaderAsync();
            // If a row is read (the customer exists).
            if (await reader.ReadAsync())
            {
                // Maps the values to a new Customer object and returns it.
                return new Customer
                {
                    CustomerID = reader.GetInt32(0),
                    Firstname = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Lastname = reader.IsDBNull(2) ? null : reader.GetString(2),
                    PhoneNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Email = reader.IsDBNull(4) ? null : reader.GetString(4)
                };
            }
            // Returns null if no customer was found with that ID.
            return null;
        }

        // Asynchronously updates an existing Customer record.
        public async Task<bool> UpdateCustomerAsync(Customer c)
        {
            // SQL query to update the customer's details based on their ID.
            const string sql = @"
                UPDATE customer
                SET first_name = @first,
                    last_name = @last,
                    phone_number = @phone,
                    email = @email
                WHERE customerID = @id;
            ";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command.
            await using var cmd = new NpgsqlCommand(sql, conn);
            // Adds parameters for all fields, handling potential nulls.
            cmd.Parameters.AddWithValue("first", c.Firstname ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("last", c.Lastname ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("phone", c.PhoneNumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("email", c.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("id", c.CustomerID);
            // Executes the command and gets the number of rows affected.
            var affected = await cmd.ExecuteNonQueryAsync();
            // Returns true if one or more rows were updated.
            return affected > 0;
        }

        // Asynchronously deletes a Customer record by its ID.
        public async Task<bool> DeleteCustomerAsync(int id)
        {
            // SQL query to delete a customer by ID.
            const string sql = "DELETE FROM customer WHERE customerID = @id;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command and adds the ID parameter.
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            // Executes the command and gets the number of rows affected.
            var affected = await cmd.ExecuteNonQueryAsync();
            // Returns true if one or more rows were deleted.
            return affected > 0;
        }

        // Asynchronously sets the 'banned' status of a customer to TRUE.
        public async Task<bool> BanCustomerAsync(int customerId)
        {
            // SQL query to update the 'banned' status.
            const string sql = "UPDATE customer SET banned = TRUE WHERE customerid = @id;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command and adds the ID parameter.
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", customerId);
            // Executes the command and gets the number of rows affected.
            var affected = await cmd.ExecuteNonQueryAsync();
            // Returns true if the update was successful.
            return affected > 0;
        }

        // Asynchronously sets the 'banned' status of a customer to FALSE.
        public async Task<bool> UnbanCustomerAsync(int customerId)
        {
            // SQL query to update the 'banned' status.
            const string sql = "UPDATE customer SET banned = FALSE WHERE customerid = @id;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command and adds the ID parameter.
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", customerId);
            // Executes the command and gets the number of rows affected.
            var affected = await cmd.ExecuteNonQueryAsync();
            // Returns true if the update was successful.
            return affected > 0;
        }

        // Asynchronously checks if a customer is banned.
        public async Task<bool> IsCustomerBannedAsync(int customerId)
        {
            // SQL query to select the 'banned' column for a specific customer.
            const string sql = "SELECT banned FROM customer WHERE customerid = @id;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command and adds the ID parameter.
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", customerId);
            // Executes the query and retrieves the scalar value (the 'banned' status).
            var obj = await cmd.ExecuteScalarAsync();
            // Returns true if the object is not null AND its converted boolean value is true.
            return obj is not null && Convert.ToBoolean(obj);
        }

        // ----------------------
        // CATEGORY CRUD
        // ----------------------

        // Asynchronously loads all Category records from the database.
        public async Task<List<Category>> LoadAllCategoriesAsync()
        {
            // Initializes an empty list for categories.
            var list = new List<Category>();
            // SQL query to select all categories, ordered by ID.
            const string sql = @"SELECT categoryID, type FROM category ORDER BY categoryID;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command.
            await using var cmd = new NpgsqlCommand(sql, conn);
            // Executes the command and gets a data reader.
            await using var reader = await cmd.ExecuteReaderAsync();
            // Reads records one by one.
            while (await reader.ReadAsync())
            {
                // Creates a new Category object and maps the column values.
                list.Add(new Category
                {
                    CategoryID = reader.GetInt32(0),
                    // Checks for DBNull before reading the string type.
                    Type = reader.IsDBNull(1) ? null : reader.GetString(1)
                });
            }
            // Returns the list of categories.
            return list;
        }

        // ----------------------
        // EQUIPMENT CRUD
        // ----------------------

        // Asynchronously inserts a new Equipment record into the database.
        public async Task<int> InsertEquipmentAsync(Equipment e)
        {
            // SQL query to insert equipment and return the generated equipmentID.
            const string sql = @"
                INSERT INTO equipment (categoryID, name, description, daily_rate, status)
                VALUES (@cat, @name, @desc, @rate, @status)
                RETURNING equipmentID;
            ";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command.
            await using var cmd = new NpgsqlCommand(sql, conn);
            // Adds parameters for equipment details, handling potential nulls for strings.
            cmd.Parameters.AddWithValue("cat", e.CategoryID);
            cmd.Parameters.AddWithValue("name", e.Name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("desc", e.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("rate", e.DailyRate);
            cmd.Parameters.AddWithValue("status", e.Status);
            // Executes the query and retrieves the new equipmentID.
            var obj = await cmd.ExecuteScalarAsync();
            // Returns the new ID if successful, otherwise -1.
            return obj is not null ? Convert.ToInt32(obj) : -1;
        }

        // Asynchronously loads all Equipment records from the database.
        public async Task<List<Equipment>> LoadAllEquipmentAsync()
        {
            // Initializes an empty list for equipment.
            var list = new List<Equipment>();
            // SQL query to select all equipment columns, ordered by ID.
            const string sql = @"SELECT equipmentID, categoryID, name, description, daily_rate, status FROM equipment ORDER BY equipmentID;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command.
            await using var cmd = new NpgsqlCommand(sql, conn);
            // Executes the command and gets a data reader.
            await using var reader = await cmd.ExecuteReaderAsync();
            // Reads records one by one.
            while (await reader.ReadAsync())
            {
                // Creates a new Equipment object and maps the column values.
                list.Add(new Equipment
                {
                    EquipmentID = reader.GetInt32(0),
                    CategoryID = reader.GetInt32(1),
                    Name = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    // Handles potential DBNull for Decimal (DailyRate).
                    DailyRate = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                    // Handles potential DBNull for Boolean (Status), defaulting to true if null.
                    Status = reader.IsDBNull(5) || reader.GetBoolean(5)
                });
            }
            // Returns the list of equipment.
            return list;
        }

        // Asynchronously loads a single Equipment record by its ID.
        public async Task<Equipment?> LoadEquipmentByIdAsync(int id)
        {
            // SQL query to select a specific equipment by its ID.
            const string sql = @"SELECT equipmentID, categoryID, name, description, daily_rate, status FROM equipment WHERE equipmentID = @id;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command and adds the ID parameter.
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            // Executes the command and gets a data reader.
            await using var reader = await cmd.ExecuteReaderAsync();
            // If a row is read (the equipment exists).
            if (await reader.ReadAsync())
            {
                // Maps the values to a new Equipment object and returns it.
                return new Equipment
                {
                    EquipmentID = reader.GetInt32(0),
                    CategoryID = reader.GetInt32(1),
                    Name = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                    DailyRate = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                    Status = reader.IsDBNull(5) || reader.GetBoolean(5)
                };
            }
            // Returns null if no equipment was found.
            return null;
        }

        // Asynchronously updates an existing Equipment record.
        public async Task<bool> UpdateEquipmentAsync(Equipment e)
        {
            // SQL query to update equipment details based on its ID.
            const string sql = @"
                UPDATE equipment
                SET categoryID = @cat,
                    name = @name,
                    description = @desc,
                    daily_rate = @rate,
                    status = @status
                WHERE equipmentID = @id;
            ";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command.
            await using var cmd = new NpgsqlCommand(sql, conn);
            // Adds parameters for all fields, handling potential nulls.
            cmd.Parameters.AddWithValue("cat", e.CategoryID);
            cmd.Parameters.AddWithValue("name", e.Name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("desc", e.Description ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("rate", e.DailyRate);
            cmd.Parameters.AddWithValue("status", e.Status);
            cmd.Parameters.AddWithValue("id", e.EquipmentID);
            // Executes the command and gets the number of rows affected.
            var affected = await cmd.ExecuteNonQueryAsync();
            // Returns true if the update was successful.
            return affected > 0;
        }

        // Asynchronously deletes an Equipment record by its ID.
        public async Task<bool> DeleteEquipmentAsync(int id)
        {
            // SQL query to delete equipment by ID.
            const string sql = "DELETE FROM equipment WHERE equipmentID = @id;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command and adds the ID parameter.
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            // Executes the command and gets the number of rows affected.
            var affected = await cmd.ExecuteNonQueryAsync();
            // Returns true if the deletion was successful.
            return affected > 0;
        }

        // ----------------------
        // RENTAL CRUD + TRANSACTIONAL CREATE
        // ----------------------

        // Asynchronously inserts a new Rental record and updates the equipment status.
        public async Task<int> InsertRentalAsync(Rental r)
        {
            // SQL query to insert the rental record and return the new rentalID.
            const string insertSql = @"
                INSERT INTO rental (customerID, equipmentID, rental_date, return_date, total_cost)
                VALUES (@customerid, @equipmentid, @rental_date, @return_date, @total_cost)
                RETURNING rentalID;
            ";

            // SQL query to update the equipment status to unavailable (false).
            const string updateEquipSql = @"UPDATE equipment SET status = @status WHERE equipmentID = @equipmentid;";

            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Checks if the connection is open.
            if (conn.State != System.Data.ConnectionState.Open) return -1;

            // Begins a database transaction.
            await using var tran = await conn.BeginTransactionAsync();
            try
            {
                // Command for the INSERT operation, associated with the connection and transaction.
                await using var cmd = new NpgsqlCommand(insertSql, conn, tran);
                // Adds parameters for the rental insert.
                cmd.Parameters.AddWithValue("customerid", r.CustomerID);
                cmd.Parameters.AddWithValue("equipmentid", r.EquipmentID);
                cmd.Parameters.AddWithValue("rental_date", r.RentalDate);
                cmd.Parameters.AddWithValue("return_date", r.ReturnDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("total_cost", r.TotalCost);

                // Executes the INSERT and gets the new rental ID.
                var idObj = await cmd.ExecuteScalarAsync();
                var newId = idObj is not null ? Convert.ToInt32(idObj) : -1;

                // Checks if the INSERT was successful.
                // mark equipment unavailable
                // Command for the UPDATE operation, associated with the connection and transaction.
                await using var cmd2 = new NpgsqlCommand(updateEquipSql, conn, tran);
                cmd2.Parameters.AddWithValue("status", false); // Sets status to false (unavailable).
                cmd2.Parameters.AddWithValue("equipmentid", r.EquipmentID);
                // Executes the UPDATE.
                await cmd2.ExecuteNonQueryAsync();

                // Commits both the INSERT and UPDATE as a single, atomic operation.
                await tran.CommitAsync();
                // Returns the new rental ID.
                return newId;
            }
            catch (Exception ex)
            {
                // If any error occurs, rolls back the transaction (undoes all changes).
                await tran.RollbackAsync();
                // Logs the error.
                Console.WriteLine($"ERROR inserting rental transaction: {ex.Message}");
                // Returns -1 to indicate failure.
                return -1;
            }
        }

        // Asynchronously loads all Rental records from the database.
        public async Task<List<Rental>> LoadAllRentalsAsync()
        {
            // Initializes an empty list for rentals.
            var list = new List<Rental>();
            // SQL query to select all rentals, ordered by ID.
            const string sql = @"SELECT rentalID, customerID, equipmentID, rental_date, return_date, total_cost FROM rental ORDER BY rentalID;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command.
            await using var cmd = new NpgsqlCommand(sql, conn);
            // Executes the command and gets a data reader.
            await using var reader = await cmd.ExecuteReaderAsync();
            // Reads records one by one.
            while (await reader.ReadAsync())
            {
                // Creates a new Rental object and maps the column values.
                list.Add(new Rental
                {
                    RentalID = reader.GetInt32(0),
                    CustomerID = reader.GetInt32(1),
                    EquipmentID = reader.GetInt32(2),
                    RentalDate = reader.GetDateTime(3),
                    // Handles potential DBNull for DateTime (ReturnDate).
                    ReturnDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    // Handles potential DBNull for Decimal (TotalCost).
                    TotalCost = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5)
                });
            }
            // Returns the list of rentals.
            return list;
        }

        // Asynchronously loads a single Rental record by its ID.
        public async Task<Rental?> LoadRentalByIdAsync(int id)
        {
            // SQL query to select a specific rental by its ID.
            const string sql = @"SELECT rentalID, customerID, equipmentID, rental_date, return_date, total_cost FROM rental WHERE rentalID = @id;";
            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Creates the command and adds the ID parameter.
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", id);
            // Executes the command and gets a data reader.
            await using var reader = await cmd.ExecuteReaderAsync();
            // If a row is read (the rental exists).
            if (await reader.ReadAsync())
            {
                // Maps the values to a new Rental object and returns it.
                return new Rental
                {
                    RentalID = reader.GetInt32(0),
                    CustomerID = reader.GetInt32(1),
                    EquipmentID = reader.GetInt32(2),
                    RentalDate = reader.GetDateTime(3),
                    ReturnDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    TotalCost = reader.IsDBNull(5) ? 0m : reader.GetDecimal(5)
                };
            }
            // Returns null if no rental was found.
            return null;
        }

        // Asynchronously updates an existing Rental record and potentially updates equipment status.
        public async Task<bool> UpdateRentalAsync(Rental r)
        {
            // SQL query to update the rental details.
            const string sql = @"
                UPDATE rental
                SET customerID = @customerid,
                    equipmentID = @equipmentid,
                    rental_date = @rental_date,
                    return_date = @return_date,
                    total_cost = @total_cost
                WHERE rentalID = @id;
            ";

            // SQL query to update the equipment status to available (true).
            const string setEquipAvailableSql = @"UPDATE equipment SET status = @status WHERE equipmentID = @equipmentid;";

            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Command for the UPDATE operation.
            await using var cmd = new NpgsqlCommand(sql, conn);
            // Adds parameters for the rental update.
            cmd.Parameters.AddWithValue("customerid", r.CustomerID);
            cmd.Parameters.AddWithValue("equipmentid", r.EquipmentID);
            cmd.Parameters.AddWithValue("rental_date", r.RentalDate);
            cmd.Parameters.AddWithValue("return_date", r.ReturnDate ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("total_cost", r.TotalCost);
            cmd.Parameters.AddWithValue("id", r.RentalID);

            // Executes the rental update and gets the number of affected rows.
            var affected = await cmd.ExecuteNonQueryAsync();

            // If the rental was updated successfully AND a return date was set (meaning the item is returned).
            if (affected > 0 && r.ReturnDate.HasValue)
            {
                // Command to set the equipment status to available (true).
                await using var cmd2 = new NpgsqlCommand(setEquipAvailableSql, conn);
                cmd2.Parameters.AddWithValue("status", true);
                cmd2.Parameters.AddWithValue("equipmentid", r.EquipmentID);
                // Executes the equipment status update.
                await cmd2.ExecuteNonQueryAsync();
            }

            // Returns true if the initial rental update was successful.
            return affected > 0;
        }

        // Asynchronously deletes a Rental record, optionally making the rented equipment available again.
        public async Task<bool> DeleteRentalAsync(int id, bool makeEquipmentAvailable = false)
        {
            // SQL query to get the equipment ID associated with the rental.
            const string getEquipSql = "SELECT equipmentid FROM rental WHERE rentalID = @id;";
            // SQL query to delete the rental record.
            const string deleteSql = "DELETE FROM rental WHERE rentalID = @id;";
            // SQL query to update the equipment status to available (true).
            const string updateEquip = "UPDATE equipment SET status = @status WHERE equipmentID = @equipmentid;";

            // Creates and opens the connection.
            await using var conn = await GetOpenConnectionAsync();
            // Begins a database transaction.
            await using var tran = await conn.BeginTransactionAsync();

            try
            {
                int equipmentId = -1;
                // Command block to retrieve the equipment ID first.
                await using (var cmdGet = new NpgsqlCommand(getEquipSql, conn, tran))
                {
                    cmdGet.Parameters.AddWithValue("id", id);
                    // Executes the query and gets the equipment ID.
                    var obj = await cmdGet.ExecuteScalarAsync();
                    if (obj is not null) equipmentId = Convert.ToInt32(obj);
                }

                // Command block to delete the rental record.
                await using (var cmdDel = new NpgsqlCommand(deleteSql, conn, tran))
                {
                    cmdDel.Parameters.AddWithValue("id", id);
                    // Executes the deletion.
                    await cmdDel.ExecuteNonQueryAsync();
                }

                // If the flag is true and a valid equipment ID was found.
                if (makeEquipmentAvailable && equipmentId > 0)
                {
                    // Command to update the equipment status.
                    await using var cmdUpd = new NpgsqlCommand(updateEquip, conn, tran);
                    cmdUpd.Parameters.AddWithValue("status", true); // Sets status to true (available).
                    cmdUpd.Parameters.AddWithValue("equipmentid", equipmentId);
                    // Executes the status update.
                    await cmdUpd.ExecuteNonQueryAsync();
                }

                // Commits the deletion and (optional) status update.
                await tran.CommitAsync();
                // Returns true indicating successful transaction.
                return true;
            }
            catch (Exception ex)
            {
                // Rolls back the transaction on error.
                await tran.RollbackAsync();
                // Logs the error.
                Console.WriteLine($"ERROR deleting rental: {ex.Message}");
                // Returns false indicating failure.
                return false;
            }
        }
    }
}