using System;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Lab3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Отримання підключення...");
            MySqlConnection conn = DBUtils.GetDBConnection();

            try
            {
                Console.WriteLine("Відкриття підключення...");

                conn.Open();

                Console.WriteLine("Підключення успішне!");

                bool running = true;
                while (running)
                {
                    Console.WriteLine("\n--- Оберіть дію " + new string('-', 34));
                    Console.WriteLine("1. Переглянути всі замовлення");
                    Console.WriteLine("2. Створити нове замовлення");
                    Console.WriteLine("3. Змінити замовлення");
                    Console.WriteLine("4. Видалити замовлення");
                    Console.WriteLine("5. Знайти транспорт для певного об’єму чи маси");
                    Console.WriteLine("6. Збільшити відстань на 20 км для замовника");
                    Console.WriteLine("7. Замовлення з підрахуванням вартості"); 
                    Console.WriteLine("8. Загальна вартість перевезень за останній місяць");
                    Console.WriteLine("9. Вид транспорту, який зовсім не замовлявся");
                    Console.WriteLine("0. Вийти");
                    
                    Console.Write("\nВведіть цифру: ");
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            ViewAllOrders(conn);
                            break;
                        case "2":
                            CreateOrder(conn);
                            break;
                        case "3":
                            UpdateOrder(conn);
                            break;
                        case "4":
                            DeleteOrder(conn);
                            break;
                        case "5":
                            FindTransport(conn);
                            break;
                        case "6":
                            UpdateDistance(conn);
                            break;
                        case "7":
                            OrderCalculation(conn);
                            break;
                        case "8":
                            TotalCostPerMonth(conn);
                            break;
                        case "9":
                            TransportNotOrdered(conn);
                            break;
                        case "0":
                            running = false;
                            break;
                        default:
                            Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Помилка: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        private static void ViewAllOrders(MySqlConnection conn)
        {
            string id, date, distance, weight, overhead, customer, transport;

            string sql = @"
                select 
                    o.order_id,
                    o.order_date,
                    o.distance,
                    o.weight,
                    o.overhead,
                    c.customer_name,
                    t.transport_name
                from transport_order o
                join customers c on o.customers_customer_id = c.customer_id
                join transport_cost t on o.transport_cost_transport_id = t.transport_id
                order by o.order_id";

            MySqlCommand cmd = new MySqlCommand();
            
            cmd.Connection = conn;
            cmd.CommandText = sql;

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    Console.WriteLine("\n--- Список замовлень " + new string('-', 29));

                    while (reader.Read())
                    {
                        id = reader["order_id"].ToString();
                        date = Convert.ToDateTime(reader["order_date"]).ToShortDateString();
                        distance = reader["distance"].ToString();
                        weight = reader["weight"].ToString();
                        overhead = reader["overhead"].ToString();
                        customer = reader["customer_name"].ToString();
                        transport = reader["transport_name"].ToString();

                        Console.WriteLine("ID: " + id);
                        Console.WriteLine("Дата: " + date);
                        Console.WriteLine("Відстань: " + distance + " км");
                        Console.WriteLine("Маса: " + weight + " т");
                        Console.WriteLine("Накладні витрати: " + overhead + " грн");
                        Console.WriteLine("Замовник: " + customer);
                        Console.WriteLine("Транспорт: " + transport);
                        Console.WriteLine(new string('-', 50));
                    }
                }
                else
                {
                    Console.WriteLine("\nНічого не знайдено.");
                }
            }
        }

        private static void CreateOrder(MySqlConnection conn)
        {
            Console.WriteLine("\n--- Створення нового замовлення " + new string('-', 18));

            DateTime orderDate;
            double distance, weight, overhead;
            int customerId, transportId;

            while (true)
            {
                Console.Write("Введіть дату замовлення (дд-мм-рррр): ");
                string input = Console.ReadLine();

                if (DateTime.TryParseExact(input, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out orderDate))
                    break;
                else
                    Console.WriteLine("Невірний формат дати. Спробуйте ще раз.");
            }

            while (true)
            {
                Console.Write("Введіть відстань: ");
                string input = Console.ReadLine();

                if (double.TryParse(input, out distance))
                    break;
                else
                    Console.WriteLine("Невірний формат. Спробуйте ще раз.");
            }

            while (true)
            {
                Console.Write("Введіть масу: ");
                string input = Console.ReadLine();

                if (double.TryParse(input, out weight))
                    break;
                else
                    Console.WriteLine("Невірний формат. Спробуйте ще раз.");
            }

            while (true)
            {
                Console.Write("Введіть накладні витрати: ");
                string input = Console.ReadLine();

                if (double.TryParse(input, out overhead))
                    break;
                else
                    Console.WriteLine("Невірний формат. Спробуйте ще раз.");
            }

            string sqlCustomers = "select customer_id, customer_name from customers order by customer_id";
            MySqlCommand cmdCustomers = new MySqlCommand(sqlCustomers, conn);
            using (DbDataReader reader = cmdCustomers.ExecuteReader())
            {
                Console.WriteLine("\n--- Список замовників " + new string('-', 28));
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["customer_id"]}. {reader["customer_name"]}");
                }
            }

            while (true)
            {
                Console.Write("\nОберіть ID замовника: ");
                if (int.TryParse(Console.ReadLine(), out customerId))
                {
                    string checkCustomerSql = $"select count(*) from customers where customer_id = {customerId}";
                    MySqlCommand checkCmd = new MySqlCommand(checkCustomerSql, conn);
                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                        break;
                    else
                        Console.WriteLine("Замовника не існує. Спробуйте ще раз.");
                }
                else
                {
                    Console.WriteLine("Невірний формат. Спробуйте ще раз.");
                }
            }

            string sqlTransport = "select transport_id, transport_name from transport_cost order by transport_id";
            MySqlCommand cmdTransport = new MySqlCommand(sqlTransport, conn);
            using (DbDataReader reader = cmdTransport.ExecuteReader())
            {
                Console.WriteLine("\n--- Список транспорту " + new string('-', 28));
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["transport_id"]}. {reader["transport_name"]}");
                }
            }

            while (true)
            {
                Console.Write("\nОберіть ID транспорту: ");
                if (int.TryParse(Console.ReadLine(), out transportId))
                {
                    string checkTransportSql = $"select count(*) from transport_cost where transport_id = {transportId}";
                    MySqlCommand checkCmd = new MySqlCommand(checkTransportSql, conn);
                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                        break;
                    else
                        Console.WriteLine("Транспорту не існує. Спробуйте ще раз.");
                }
                else
                {
                    Console.WriteLine("Невірний формат. Спробуйте ще раз.");
                }
            }

            string sql = @"
                insert into transport_order 
                (order_date, distance, weight, overhead, customers_customer_id, transport_cost_transport_id)
                values (@date, @distance, @weight, @overhead, @customerId, @transportId)";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@date", orderDate);
            cmd.Parameters.AddWithValue("@distance", distance);
            cmd.Parameters.AddWithValue("@weight", weight);
            cmd.Parameters.AddWithValue("@overhead", overhead);
            cmd.Parameters.AddWithValue("@customerId", customerId);
            cmd.Parameters.AddWithValue("@transportId", transportId);

            try
            {
                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                {
                    long newOrderId = cmd.LastInsertedId;
                    Console.WriteLine($"\nЗамовлення №{newOrderId} успішно створено.");
                }
                else
                {
                    Console.WriteLine("\nСталася помилка при створенні замовлення.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nПомилка: " + ex.Message);
            }
        }

        private static void UpdateOrder(MySqlConnection conn)
        {
            string sql = @"
                select 
                    o.order_id, o.order_date, o.distance, o.weight, o.overhead, 
                    c.customer_name, t.transport_name
                from transport_order o
                join customers c on o.customers_customer_id = c.customer_id
                join transport_cost t on o.transport_cost_transport_id = t.transport_id
                order by o.order_id";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    Console.WriteLine("\n--- Список замовлень " + new string('-', 29));
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["order_id"]}");
                        Console.WriteLine($"Дата: {Convert.ToDateTime(reader["order_date"]).ToString("dd.MM.yyyy")}");
                        Console.WriteLine($"Відстань: {reader["distance"]} км");
                        Console.WriteLine($"Маса: {reader["weight"]} т");
                        Console.WriteLine($"Замовник: {reader["customer_name"]}");
                        Console.WriteLine($"Транспорт: {reader["transport_name"]}");
                        Console.WriteLine(new string('-', 50));
                    }
                }
                else
                {
                    Console.WriteLine("Замовлень не знайдено.");
                    return;
                }
            }

            int orderId;
            while (true)
            {
                Console.Write("\nВведіть ID замовлення для зміни: ");
                if (int.TryParse(Console.ReadLine(), out orderId))
                {
                    string checkSql = "select count(*) from transport_order where order_id = @orderId";
                    MySqlCommand checkCmd = new MySqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@orderId", orderId);
                    long count = (long)checkCmd.ExecuteScalar();

                    if (count > 0) break;
                    Console.WriteLine("Такого замовлення не існує.");
                }
                else Console.WriteLine("Невірний формат.");
            }

            DateTime? newDate = null;
            double? newDistance = null;
            double? newWeight = null;
            int? customerId = null;
            int? transportId = null;

            Console.WriteLine("\nЩо ви хочете змінити?");
            Console.WriteLine("1. Дата");
            Console.WriteLine("2. Відстань");
            Console.WriteLine("3. Маса");
            Console.WriteLine("4. Замовник");
            Console.WriteLine("5. Транспорт");
            Console.Write("\nВведіть номери змін через кому (наприклад: 1, 3, 5): ");

            string[] choices = Console.ReadLine()?.Split(',') ?? Array.Empty<string>();

            foreach (string choice in choices.Select(c => c.Trim()))
            {
                switch (choice)
                {
                    case "1":
                        while (true)
                        {
                            Console.Write("Нова дата (дд-мм-рррр): ");
                            if (DateTime.TryParseExact(Console.ReadLine(), "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dt))
                            {
                                newDate = dt;
                                break;
                            }
                            Console.WriteLine("Невірний формат.");
                        }
                        break;
                    case "2":
                        while (true)
                        {
                            Console.Write("Нова відстань: ");
                            if (double.TryParse(Console.ReadLine(), out double dist) && dist >= 0)
                            {
                                newDistance = dist;
                                break;
                            }
                            Console.WriteLine("Невірний формат.");
                        }
                        break;
                    case "3":
                        while (true)
                        {
                            Console.Write("Нова маса: ");
                            if (double.TryParse(Console.ReadLine(), out double w) && w >= 0)
                            {
                                newWeight = w;
                                break;
                            }
                            Console.WriteLine("Невірний формат.");
                        }
                        break;
                    case "4":
                        Console.WriteLine("\n--- Список замовників " + new string('-', 28));
                        using (MySqlCommand cmdCustomers = new MySqlCommand("select customer_id, customer_name from customers order by customer_id", conn))
                        using (DbDataReader custReader = cmdCustomers.ExecuteReader())
                        {
                            while (custReader.Read())
                            {
                                Console.WriteLine($"{custReader["customer_id"]}. {custReader["customer_name"]}");
                            }
                        }
                        while (true)
                        {
                            Console.Write("Оберіть ID замовника: ");
                            if (int.TryParse(Console.ReadLine(), out int cid))
                            {
                                customerId = cid;
                                break;
                            }
                            Console.WriteLine("Невірний формат.");
                        }
                        break;
                    case "5":
                        Console.WriteLine("\n--- Список транспорту " + new string('-', 28));
                        using (MySqlCommand cmdTransport = new MySqlCommand("select transport_id, transport_name from transport_cost order by transport_id", conn))
                        using (DbDataReader trReader = cmdTransport.ExecuteReader())
                        {
                            while (trReader.Read())
                            {
                                Console.WriteLine($"{trReader["transport_id"]}. {trReader["transport_name"]}");
                            }
                        }
                        while (true)
                        {
                            Console.Write("Оберіть ID транспорту: ");
                            if (int.TryParse(Console.ReadLine(), out int tid))
                            {
                                transportId = tid;
                                break;
                            }
                            Console.WriteLine("Невірний формат.");
                        }
                        break;
                    default:
                        Console.WriteLine($"Невірна опція: {choice}");
                        break;
                }
            }

            List<string> updates = new();
            MySqlCommand updateCmd = new MySqlCommand();
            if (newWeight.HasValue) { updates.Add("weight = @weight"); updateCmd.Parameters.AddWithValue("@weight", newWeight.Value); }
            if (newDistance.HasValue) { updates.Add("distance = @distance"); updateCmd.Parameters.AddWithValue("@distance", newDistance.Value); }
            if (newDate.HasValue) { updates.Add("order_date = @date"); updateCmd.Parameters.AddWithValue("@date", newDate.Value.ToString("yyyy-MM-dd")); }
            if (customerId.HasValue) { updates.Add("customers_customer_id = @customerId"); updateCmd.Parameters.AddWithValue("@customerId", customerId.Value); }
            if (transportId.HasValue) { updates.Add("transport_cost_transport_id = @transportId"); updateCmd.Parameters.AddWithValue("@transportId", transportId.Value); }

            if (updates.Count == 0)
            {
                Console.WriteLine("Жодного поля не вибрано для оновлення.");
                return;
            }

            string updateSql = $"update transport_order set {string.Join(", ", updates)} where order_id = @orderId";
            updateCmd.CommandText = updateSql;
            updateCmd.Connection = conn;
            updateCmd.Parameters.AddWithValue("@orderId", orderId);

            try
            {
                int affected = updateCmd.ExecuteNonQuery();
                if (affected > 0)
                    Console.WriteLine("\nЗапис успішно оновлено.");
                else
                    Console.WriteLine("\nОновлення не виконано.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка: " + ex.Message);
            }
        }

        private static void DeleteOrder(MySqlConnection conn)
        {
            string sql = @"
                select 
                    o.order_id, o.order_date, o.distance, o.weight, o.overhead, 
                    c.customer_name, t.transport_name
                from transport_order o
                join customers c on o.customers_customer_id = c.customer_id
                join transport_cost t on o.transport_cost_transport_id = t.transport_id
                order by o.order_id";

            MySqlCommand cmd = new MySqlCommand(sql, conn);
            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    Console.WriteLine("\n--- Список замовлень " + new string('-', 29));
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["order_id"]}");
                        Console.WriteLine($"Дата: {Convert.ToDateTime(reader["order_date"]).ToString("dd.MM.yyyy")}");
                        Console.WriteLine($"Відстань: {reader["distance"]} км");
                        Console.WriteLine($"Маса: {reader["weight"]} т");
                        Console.WriteLine($"Накладні витрати: {reader["overhead"]} грн");
                        Console.WriteLine($"Замовник: {reader["customer_name"]}");
                        Console.WriteLine($"Транспорт: {reader["transport_name"]}");
                        Console.WriteLine(new string('-', 50));
                    }
                }
                else
                {
                    Console.WriteLine("\nЗамовлень не знайдено.");
                    return;
                }
            }
            
            int orderId;
            while (true)
            {
                Console.Write("\nВведіть ID замовлення для видалення: ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out orderId))
                {
                    string checkSql = "select count(*) from transport_order where order_id = @orderId";
                    MySqlCommand checkCmd = new MySqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@orderId", orderId);
                    long count = (long)checkCmd.ExecuteScalar();

                    if (count > 0)
                        break;
                    else
                        Console.WriteLine("Замовлення не існує. Спробуйте ще раз.");
                }
                else
                {
                    Console.WriteLine("Невірний формат. Спробуйте ще раз.");
                }
            }

            Console.Write("Ви впевнені, що хочете видалити це замовлення? (так/ні): ");
            string confirm = Console.ReadLine()?.Trim().ToLower();
            if (confirm != "так")
            {
                Console.WriteLine("Видалення скасовано.");
                return;
            }

            string deleteSql = "delete from transport_order where order_id = @orderId";
            MySqlCommand deleteCmd = new MySqlCommand(deleteSql, conn);
            deleteCmd.Parameters.AddWithValue("@orderId", orderId);

            try
            {
                int affectedRows = deleteCmd.ExecuteNonQuery();
                if (affectedRows > 0)
                    Console.WriteLine("\nЗамовлення успішно видалено.");
                else
                    Console.WriteLine("\nПомилка при видаленні.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nПомилка: " + ex.Message);
            }
        }

        private static void FindTransport(MySqlConnection conn)
        {
            string id, name, cost, volume, weight;

            double doubleVolume, doubleWeight;

            while (true)
            {
                Console.Write("\nВведіть об’єм вантажу: ");
                string strVolume = Console.ReadLine();

                if (double.TryParse(strVolume, out doubleVolume))
                    break;
                else
                    Console.WriteLine("Невірний формат. Спробуйте ще раз.");
            }

            while (true)
            {
                Console.Write("Введіть масу вантажу: ");
                string strWeight = Console.ReadLine();

                if (double.TryParse(strWeight, out doubleWeight))
                    break;
                else
                    Console.WriteLine("Невірний формат. Спробуйте ще раз.\n");
            }

            string sql = @"
                select 
                    transport_id,
                    transport_name,
                    cost,
                    cargo_volume,
                    cargo_weight
                from transport_cost
                where cargo_weight >= @weight and cargo_volume >= @volume";

            MySqlCommand cmd = new MySqlCommand();
            
            cmd.Connection = conn;
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@weight", doubleWeight);
            cmd.Parameters.AddWithValue("@volume", doubleVolume);

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    Console.WriteLine("\n--- Транспорт для певного об’єму чи маси " + new string('-', 10));

                    while (reader.Read())
                    {
                        id = reader["transport_id"].ToString();
                        name = reader["transport_name"].ToString();
                        cost = reader["cost"].ToString();
                        volume = reader["cargo_volume"].ToString();
                        weight = reader["cargo_weight"].ToString();

                        Console.WriteLine("ID: " + id);
                        Console.WriteLine("Назва: " + name);
                        Console.WriteLine("Вартість т/км: " + cost + " грн");
                        Console.WriteLine("Об’єм вантажу: " + volume + " м3");
                        Console.WriteLine("Маса вантажу: " + weight + " т");
                        Console.WriteLine(new string('-', 50));
                    }
                }
                else
                {
                    Console.WriteLine("\nНічого не знайдено.");
                }
            }
        }

        private static void UpdateDistance(MySqlConnection conn)
        {
            string id, name;

            string sql = @"
                select 
                    customer_id, 
                    customer_name 
                from customers 
                order by customer_id";

            MySqlCommand cmd = new MySqlCommand();
            
            cmd.Connection = conn;
            cmd.CommandText = sql;

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    Console.WriteLine("\n--- Список замовників " + new string('-', 28));
                    while (reader.Read())
                    {
                        id = reader["customer_id"].ToString();
                        name = reader["customer_name"].ToString();

                        Console.WriteLine($"{id}. {name}");
                    }
                    Console.WriteLine(new string('-', 50));
                }
                else
                {
                    Console.WriteLine("\nНічого не знайдено.");
                    return;
                }
            }

            int intCustomerId;
            string customerName;

            while (true)
            {
                Console.Write("\nВведіть номер замовника: ");
                string strСustomerId = Console.ReadLine();

                if (!int.TryParse(strСustomerId, out intCustomerId))
                {
                    Console.WriteLine("Невірний формат. Спробуйте ще раз.");
                    continue;
                }

                string sql2 = @"
                    select 
                        customer_name 
                    from customers 
                    where customer_id = " + intCustomerId;

                MySqlCommand cmd2 = new MySqlCommand();

                cmd2.Connection = conn;
                cmd2.CommandText = sql2;

                object result = cmd2.ExecuteScalar();

                if (result == null)
                {
                    Console.WriteLine("Замовника не існує. Спробуйте ще раз.");
                    continue;
                }
                else
                {
                    customerName = result.ToString();
                    break;
                }
            }

            string sql3 = @"
                update transport_order 
                set distance = distance + 20 
                where customers_customer_id = " + intCustomerId;

            MySqlCommand cmd3 = new MySqlCommand();

            cmd3.Connection = conn;
            cmd3.CommandText = sql3;

            int rowsAffected = cmd3.ExecuteNonQuery();

            Console.WriteLine("\nВідстань збільшено на 20 км для замовника: " + customerName);
        }

        private static void OrderCalculation(MySqlConnection conn)
        {
            string id, distance, weight, overhead, rate, customer, transport;

            string sql = @"
                select 
                    o.order_id,
                    o.distance,
                    o.weight,
                    o.overhead,
                    t.cost AS rate,
                    c.customer_name,
                    t.transport_name
                from transport_order o
                join transport_cost t on o.transport_cost_transport_id = t.transport_id
                join customers c on o.customers_customer_id = c.customer_id
                order by o.order_id";

            MySqlCommand cmd = new MySqlCommand();
            
            cmd.Connection = conn;
            cmd.CommandText = sql;

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    Console.WriteLine("\n--- Замовлення з підрахуванням вартості " + new string('-', 10));

                    while (reader.Read())
                    {
                        id = reader["order_id"].ToString();
                        distance = reader["distance"].ToString();
                        weight = reader["weight"].ToString();
                        overhead = reader["overhead"].ToString();
                        rate = reader["rate"].ToString();
                        customer = reader["customer_name"].ToString();
                        transport = reader["transport_name"].ToString();

                        decimal d = Convert.ToDecimal(distance);
                        decimal w = Convert.ToDecimal(weight);
                        decimal o = Convert.ToDecimal(overhead);
                        decimal r = Convert.ToDecimal(rate);

                        decimal discount = 0;
                        if (d >= 100 && d < 1000)
                            discount = 0.05m;
                        else if (d >= 1000 && d < 5000)
                            discount = 0.10m;
                        else if (d >= 5000)
                            discount = 0.15m;

                        decimal base_cost = d * r * w + o;
                        decimal with_discount = base_cost - (base_cost * discount);

                        Console.WriteLine("ID: " + id);
                        Console.WriteLine("Замовник: " + customer);
                        Console.WriteLine("Транспорт: " + transport);
                        Console.WriteLine("Відстань: " + distance + " км");
                        Console.WriteLine("Маса: " + weight + " т");
                        Console.WriteLine("Вартість т/км: " + rate + " грн");
                        Console.WriteLine("Накладні витрати: " + overhead + " грн");
                        Console.WriteLine($"Ціна без знижки: {base_cost:F2} грн");
                        Console.WriteLine($"Знижка: {discount * 100} %");
                        Console.WriteLine($"Ціна зі знижкою: {with_discount:F2} грн");
                        Console.WriteLine(new string('-', 50));
                    }
                }
                else
                {
                    Console.WriteLine("\nНічого не знайдено.");
                }
            }
        }

        static void TotalCostPerMonth(MySqlConnection conn)
        {
            string distance, weight, overhead, rate;

            string sql = @"
                select 
                    o.distance,
                    o.weight,
                    o.overhead,
                    o.order_date,
                    t.cost AS rate
                from transport_order o
                join transport_cost t on o.transport_cost_transport_id = t.transport_id
                where o.order_date >= date_sub(curdate(), interval 1 month)";

            MySqlCommand cmd = new MySqlCommand();
            
            cmd.Connection = conn;
            cmd.CommandText = sql;

            decimal total = 0;

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        distance = reader["distance"].ToString();
                        weight = reader["weight"].ToString();
                        overhead = reader["overhead"].ToString();
                        rate = reader["rate"].ToString();

                        decimal d = Convert.ToDecimal(distance);
                        decimal w = Convert.ToDecimal(weight);
                        decimal o = Convert.ToDecimal(overhead);
                        decimal r = Convert.ToDecimal(rate);

                        decimal discount = 0;
                        if (d >= 100 && d < 1000)
                            discount = 0.05m;
                        else if (d >= 1000 && d < 5000)
                            discount = 0.10m;
                        else if (d >= 5000)
                            discount = 0.15m;

                        decimal base_cost = d * r * w + o;
                        decimal with_discount = base_cost - (base_cost * discount);
                        
                        total += with_discount;
                    }
                    Console.WriteLine($"\nЗагальна вартість перевезень за останній місяць: {total:F2} грн");
                }
                else
                {
                    Console.WriteLine("\nНічого не знайдено.");
                }
            }
        }

        private static void TransportNotOrdered(MySqlConnection conn)
        {
            string id, name;

            string sql = @"
                select 
                    transport_id,
                    transport_name
                from transport_cost
                where transport_id not in (select distinct transport_cost_transport_id from transport_order)";

            MySqlCommand cmd = new MySqlCommand();
            
            cmd.Connection = conn;
            cmd.CommandText = sql;

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    Console.WriteLine("\n--- Вид транспорту, який зовсім не замовлявся " + new string('-', 4));

                    while (reader.Read())
                    {
                        id = reader["transport_id"].ToString();
                        name = reader["transport_name"].ToString();

                        Console.WriteLine("ID: " + id);
                        Console.WriteLine("Назва: " + name);
                        Console.WriteLine(new string('-', 50));
                    }
                }
                else
                {
                    Console.WriteLine("\nНічого не знайдено.");
                }
            }
        }
    }
}