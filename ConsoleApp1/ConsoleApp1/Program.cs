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
                    Console.WriteLine("2. Знайти транспорт для певного об’єму чи маси");
                    Console.WriteLine("3. Збільшити відстань на 20 км для замовника");
                    Console.WriteLine("4. Замовлення з підрахуванням вартості"); 
                    Console.WriteLine("5. Загальна вартість перевезень за останній місяць");
                    Console.WriteLine("6. Вид транспорту, який зовсім не замовлявся");
                    Console.WriteLine("0. Вийти");
                    
                    Console.Write("Введіть цифру: ");
                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            ViewAllOrders(conn);
                            break;
                        case "2":
                            FindTransport(conn);
                            break;
                        case "3":
                            UpdateDistance(conn);
                            break;
                        case "4":
                            OrderCalculation(conn);
                            break;
                        case "5":
                            TotalCostPerMonth(conn);
                            break;
                        case "6":
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

        private static void FindTransport(MySqlConnection conn)
        {
            string id, name, cost, volume, weight;

            int intVolume, intWeight;

            while (true)
            {
                Console.Write("\nВведіть об’єм вантажу: ");
                string strVolume = Console.ReadLine();

                if (int.TryParse(strVolume, out intVolume))
                    break;
                else
                    Console.WriteLine("Невірний формат. Спробуйте ще раз.");
            }

            while (true)
            {
                Console.Write("Введіть масу вантажу: ");
                string strWeight = Console.ReadLine();

                if (int.TryParse(strWeight, out intWeight))
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
                where cargo_weight >= " + intWeight + " and cargo_volume >= " + intVolume;

            MySqlCommand cmd = new MySqlCommand();
            
            cmd.Connection = conn;
            cmd.CommandText = sql;

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

            int intСustomerId;
            string customerName;

            while (true)
            {
                Console.Write("\nВведіть номер замовника: ");
                string strСustomerId = Console.ReadLine();

                if (!int.TryParse(strСustomerId, out intСustomerId))
                {
                    Console.WriteLine("Невірний формат. Спробуйте ще раз.");
                    continue;
                }

                string sql2 = @"
                    select 
                        customer_name 
                    from customers 
                    where customer_id = " + intСustomerId;
                
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
                where customers_customer_id = " + intСustomerId;

            MySqlCommand cmd3 = new MySqlCommand();

            cmd3.Connection = conn;
            cmd3.CommandText = sql3;

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