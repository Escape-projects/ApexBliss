using Dapper;
using Microsoft.Data.SqlClient;

string connectionString = @"Data Source=localhost;Initial Catalog=ComputerClub;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=""SQL Server Management Studio"";Command Timeout=0";
IDBComputers computers = new SQLTableComputers(connectionString);
IDBClients clients = new SQLTableClients(connectionString);
IDBSessions sessions = new SQLTableSessions(connectionString);
ComputerClub computerClub = new ComputerClub(computers, clients, sessions);

while (true)
{
    computerClub.DisplayInfo(true);
    string selectedOption = Console.ReadLine();

    switch (selectedOption)
    {
        case "1":
            computerClub.AddComputers();
            break;

        case "2":
            computerClub.DeleteComputers();
            break;

        case "3":
            computerClub.StartSession();
            break;

        case "4":
            computerClub.EndSession();
            break;

        case "выход":
            computerClub.Close();
            break;

        default:
            Console.WriteLine("Такой опции нет");
            MyFunctions.PressAnyKeyToContinue();
            break;
    }
}

public static class MyFunctions
{
    public static int TryStringToInt(string String)
    {
        int answer = 0;
        try
        {
            answer = int.Parse(String);
            return answer;
        }
        catch
        {
            return answer;
        }
    }

    public static void PressAnyKeyToContinue()
    {
        Console.WriteLine("\nнажми любую кнопку что бы продолжить");
        Console.ReadKey();
    }
}

class ComputerClub
{
    List<Computer> computers;
    List<Client> clients;
    List<Session> sessions;

    IDBComputers db_computers;
    IDBClients db_clients;
    IDBSessions db_sessions;

    public ComputerClub(IDBComputers DBComputers, IDBClients DBClients, IDBSessions DBSessions)
    {
        db_computers = DBComputers;
        db_clients = DBClients;
        db_sessions = DBSessions;
    }

    void loadDataBase()
    {
        computers = db_computers.GetAll();
        clients = db_clients.GetAll();
        sessions = db_sessions.GetAll();
    }

    public void DisplayInfo(bool showOptions)
    {
        loadDataBase();
        Console.Clear();
        Console.WriteLine($"Компьютерный клуб ApexBliss\n" +
                          $"Кол-во пк: {GetCount(Type.ComputersAll)} | свободно: {GetCount(Type.ComputersAvailable)}\n" +
                          $"Кол-во клиентов в клубе: {GetCount(Type.ClientsInClub)}");

        if (showOptions) Console.WriteLine("\nОпции:\n1.Добавить пк\n2.Удалить пк\n3.Начать сессию\n4.Завершить сессию\n");
    }

    public enum Type { ComputersAll, ComputersAvailable, ClientsInClub, ActiveSessions }

    int GetCount(Type type)
    {
        loadDataBase();
        switch (type)
        {
            case Type.ComputersAll: return computers.Count;
            case Type.ComputersAvailable: return computers.Count(computer => computer.status == Computer.Status.Free);
            case Type.ClientsInClub: return clients.Count(clients => clients.status == Client.Status.AtComputer);
            default: return 0;
        }
    }

    string GetString(Type type)
    {
        loadDataBase();
        string answer = "";
        int number = 1;
        switch (type)
        {
            case Type.ComputersAll:
                foreach (var computer in computers)
                {
                    answer += $"{number}) {computer.name} | {Computer.StatusString(computer.status)}\n";
                    number++;
                }
                return answer;
            case Type.ComputersAvailable:
                foreach (var computer in computers)
                {
                    if (computer.status == Computer.Status.Free)
                    {
                        answer += $"{number}) {computer.name}\n";
                        number++;
                    }
                }
                return answer;
            case Type.ActiveSessions:
                if (sessions.Count == 0) return "0 активных сессий";
                foreach (var session in sessions) if (session.ended_at == null) answer += $"Сессия #{session.id} | пк: {session.computer.name} | клиент: {session.client.name}\n";
                return answer;
            default: return answer;
        }
    }

    string GetClientSittingAtComputer(string computerName)
    {
        Client client = db_clients.GetSitterAtComputer(computerName);
        if (client != default) return client.name;
        else return "ошибка";
    }

    string GetComputerUsingClient(string clientName)
    {
        Computer computer = db_computers.GetBySittingAtClient(clientName);
        if (computer != null) return computer.name;
        else return "ошибка";
    }

    public void AddComputers()
    {
        bool exit = false;
        while (!exit)
        {
            DisplayInfo(false);
            Console.WriteLine("пк клуба:");
            Console.Write($"{GetString(Type.ComputersAll)}\n1.Добавить пк:\nКол-во пк: ");
            string input = Console.ReadLine();
            if (input != "выход")
            {
                int computersCount = MyFunctions.TryStringToInt(input);
                if (computersCount > 0 && computersCount <= 10)
                {
                    for (int i = 0; i < computersCount; i++)
                    {
                        Console.Write($"{i + 1}.Название: ");
                        string pcName = Console.ReadLine();
                        switch (db_computers.Add(pcName))
                        {
                            case Answer.Successfully:
                                Console.WriteLine($"пк {pcName}: добавлен");
                                break;
                            case Answer.NameBusy:
                                Console.WriteLine($"пк {pcName}: уже существует");
                                i--;
                                break;
                            case Answer.Error:
                                Console.WriteLine($"ошибка при добавлении пк в базу данных, обратитесь к админу");
                                i--;
                                break;
                        }
                    }
                    MyFunctions.PressAnyKeyToContinue();
                    exit = true;
                }
                else
                {
                    Console.WriteLine("За один раз можно добавить от 1 до 10 пк");
                    MyFunctions.PressAnyKeyToContinue();
                }
            }
            else exit = true;
        }
    }

    public void DeleteComputers()
    {
        bool exit = false;
        while (!exit)
        {
            DisplayInfo(false);
            Console.WriteLine("\nпк клуба:");
            Console.Write($"{GetString(Type.ComputersAll)}\n2.Удалить пк:\nКол-во пк: ");
            string input = Console.ReadLine();
            if (input != "выход")
            {
                int computersCount = MyFunctions.TryStringToInt(input);
                if (computersCount > 0 && computersCount <= 10)
                {
                    for (int i = 0; i < computersCount; i++)
                    {
                        Console.Write($"{i + 1}.Название: ");
                        string pcName = Console.ReadLine();

                        switch (db_computers.Delete(pcName))
                        {
                            case Answer.Successfully:
                                Console.WriteLine($"пк {pcName}: удален");
                                break;
                            case Answer.NoneComputer:
                                Console.WriteLine($"пк {pcName}: не найден в бд");
                                i--;
                                break;
                            case Answer.ComputerBusy:
                                Console.WriteLine($"пк {pcName}: занят клиентом {GetClientSittingAtComputer(pcName)}");
                                i--;
                                break;
                            case Answer.Error:
                                Console.WriteLine($"ошибка при добавлении пк в базу данных, обратитесь к админу");
                                i--;
                                break;
                        }
                    }
                    MyFunctions.PressAnyKeyToContinue();
                    exit = true;
                }
                else
                {
                    Console.WriteLine("За один раз можно удалить от 1 до 10 пк");
                    MyFunctions.PressAnyKeyToContinue();
                }
            }
            else exit = true;
        }
    }

    public void StartSession()
    {
        bool exit = false;
        while (!exit)
        {
            DisplayInfo(false);
            Console.Write($"\nСвободные пк:\n{GetString(Type.ComputersAvailable)}\n3.Начать сессию\nпк: ");
            string computerName = Console.ReadLine();
            if (computerName != "выход")
            {
                Computer pc = computers.Find(pc => pc.name == computerName);
                if (pc != default && pc.status == Computer.Status.Free)
                {
                    Console.Write("user name клиента: ");
                    string clientName = Console.ReadLine();
                    if (clientName != "выход")
                    {
                        switch (db_clients.Add(clientName))
                        {
                            case Answer.Successfully:
                            case Answer.ClientNotAtComputer:
                                loadDataBase();
                                int ComputerId = computers.Find(computer => computer.name == computerName).id;
                                int ClientId = clients.Find(client => client.name == clientName).id;

                                switch (db_sessions.Start(ComputerId, ClientId))
                                {
                                    case Answer.Successfully:
                                        Console.WriteLine($"клиент {clientName}: занял пк {computerName}");
                                        MyFunctions.PressAnyKeyToContinue();
                                        exit = true;
                                        break;
                                    case Answer.Error:
                                        Console.WriteLine($"ошибка записи новой сесии в бд, обратитесь к админу");
                                        MyFunctions.PressAnyKeyToContinue();
                                        exit = true;
                                        break;
                                }
                                break;

                            case Answer.ClientAtComputer:
                                Console.WriteLine($"клиент {clientName}: уже сидит за пк {GetComputerUsingClient(clientName)}");
                                MyFunctions.PressAnyKeyToContinue();
                                break;

                            case Answer.Error:
                                Console.WriteLine($"ошибка при добавлении нового клиента в бд, обратитесь к админу");
                                MyFunctions.PressAnyKeyToContinue();
                                break;
                        }
                    }
                    else exit = true;
                }
                else
                {
                    if (pc == default) Console.WriteLine("пк не найден в бд");
                    else if (pc.status == Computer.Status.Busy) Console.WriteLine($"пк {pc.name}: занят клиентом {GetClientSittingAtComputer(pc.name)}");
                    MyFunctions.PressAnyKeyToContinue();
                }
            }
            else exit = true;
        }
    }

    public void EndSession()
    {
        bool exit = false;
        while (!exit)
        {
            DisplayInfo(false);
            Console.Write($"\nАктивные сессии:\n{GetString(Type.ActiveSessions)}\n4.Закончить сессию\nсессия #: ");
            string input = Console.ReadLine();
            if (input != "выход")
            {
                int number = MyFunctions.TryStringToInt(input);
                if (number > 0)
                {
                    switch (db_sessions.End(number))
                    {
                        case Answer.Successfully:
                            loadDataBase();
                            Session session = sessions.Find(session => session.id == number);
                            string computer_name = session.computer.name;
                            string client_name = session.client.name;
                            string started_at = session.started_at.ToString("dd.MM.yyyy HH:mm");
                            string ended_at = session.ended_at.Value.ToString("dd.MM.yyyy HH:mm");

                            Console.WriteLine($"\nСессия #{number} | пк: {computer_name} | клиент: {client_name} | начата: {started_at} | закончена: {ended_at}");
                            MyFunctions.PressAnyKeyToContinue();
                            exit = true;
                            break;
                        case Answer.NoneSession:
                            Console.WriteLine($"нет активной сессии #{number}");
                            MyFunctions.PressAnyKeyToContinue();
                            break;
                        case Answer.Error:
                            Console.WriteLine("ошибка в бд, обратитесь к сис админу");
                            MyFunctions.PressAnyKeyToContinue();
                            exit = true;
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("# сессии не может быть 0 и меньше");
                    MyFunctions.PressAnyKeyToContinue();
                }
            }
            else exit = true;
        }
    }

    public void Close()
    {
        Environment.Exit(0);
    }
}

class Computer
{
    public int id { get; private set; }
    public string name { get; private set; }
    public enum Status { Free, Busy }
    public Status status { get; private set; }
    public const string Free = "свободен", Busy = "занят";

    public Computer(int Id, string Name, Status Status)
    {
        id = Id;
        name = Name;
        status = Status;
    }

    static public string StatusString(Status status)
    {
        switch (status)
        {
            case Status.Free: return Free;
            case Status.Busy: return Busy;
            default: throw new ArgumentException($"Неизвестный статус: {status}");
        }
    }

    static public Status StatusEnum(string status)
    {
        switch (status)
        {
            case Free: return Status.Free;
            case Busy: return Status.Busy;
            default: throw new ArgumentException($"Неизвестный статус: {status}");
        }
    }
}

class Client
{
    public int id { get; private set; }
    public string name { get; private set; }
    public enum Status { AtComputer, NotAtComputer }
    public Status status { get; private set; }
    public const string AtComputer = "за пк", NotAtComputer = "не за пк";

    public Client(int ID, string Name, Status Status)
    {
        id = ID;
        name = Name;
        status = Status;
    }

    static public string StatusString(Status status)
    {
        switch (status)
        {
            case Status.AtComputer: return AtComputer;
            case Status.NotAtComputer: return NotAtComputer;
            default: throw new ArgumentException($"Неизвестный статус: {status}");
        }
    }

    static public Status StatusEnum(string status)
    {
        switch (status)
        {
            case AtComputer: return Status.AtComputer;
            case NotAtComputer: return Status.NotAtComputer;
            default: throw new ArgumentException($"Неизвестный статус: {status}");
        }
    }
}

class Session
{
    public int id { get; private set; }
    public Computer computer { get; private set; }
    public Client client { get; private set; }
    public DateTimeOffset started_at { get; private set; }
    public DateTimeOffset? ended_at { get; private set; }

    public Session(int Id, Computer Computer, Client Client, DateTimeOffset StartedAt, DateTimeOffset? EndedAt)
    {
        id = Id;
        computer = Computer;
        client = Client;
        started_at = StartedAt;
        ended_at = EndedAt;
    }
}

class ComputerDTO
{
    public int computer_id;
    public string computer_name;
    public string computer_status;
}

class ClientDTO
{
    public int client_id;
    public string client_name;
    public string client_status;
}

class SessionDTO
{
    public int session_id;
    public int computer_id;
    public int client_id;
    public DateTimeOffset session_started_at;
    public DateTimeOffset? session_ended_at;
}

public enum Answer { Error, Successfully, NameBusy, ComputerBusy, NoneComputer, ClientAtComputer, ClientNotAtComputer, NoneSession }

class SQLTableComputers : IDBComputers
{
    string connectionString;

    public SQLTableComputers(string ConnectionString)
    {
        connectionString = ConnectionString;
    }

    public Answer Add(string Name)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            bool nameIsAvailable = connection.QueryFirstOrDefault<string>("select computer_name from Computers where computer_name = @name", new { name = Name }) == null;
            if (nameIsAvailable)
            {
                int rows = connection.Execute("insert into Computers(computer_name) values(@name)", new { name = Name });
                return rows > 0 ? Answer.Successfully : Answer.Error;
            }
            else return Answer.NameBusy;
        }
    }

    public Answer Delete(string Name)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string computerStatus = connection.QueryFirstOrDefault<string>("select computer_status from Computers where computer_name = @name", new { name = Name });
            if (computerStatus != null && computerStatus != Computer.Status.Busy.ToString())
            {
                int rows = connection.Execute("delete from Computers where computer_name = @name", new { name = Name });
                return rows > 0 ? Answer.Successfully : Answer.Error;
            }
            else return computerStatus == null ? Answer.NoneComputer : computerStatus == Computer.Status.Busy.ToString() ? Answer.ComputerBusy : Answer.Error;
        }
    }

    public List<Computer> GetAll()
    {
        List<Computer> computers = new List<Computer>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            List<ComputerDTO> computersDTO = connection.Query<ComputerDTO>("select * from Computers").ToList();
            if (computersDTO.Count > 0) foreach (var computer in computersDTO) computers.Add(new Computer(computer.computer_id, computer.computer_name, Computer.StatusEnum(computer.computer_status)));
            return computers;
        }
    }

    public Computer GetBySittingAtClient(string ClientName)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string query = $"select pc.* from Computers pc " +
                           $"join Sessions s on pc.computer_id = s.computer_id " +
                           $"join Clients c on s.client_id = c.client_id " +
                           $"where c.client_name = @client_name and s.session_ended_at is null";
            var computerDTO = connection.QueryFirstOrDefault<ComputerDTO>(query, new { client_name = ClientName });
            return computerDTO != null ? new Computer(computerDTO.computer_id, computerDTO.computer_name, Computer.StatusEnum(computerDTO.computer_status)) : null;
        }
    }
}

class SQLTableClients : IDBClients
{
    string connectionString;

    public SQLTableClients(string ConnectionString)
    {
        connectionString = ConnectionString;
    }

    public Answer Add(string Name)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string client_status = connection.QueryFirstOrDefault<string>("select client_status from Clients where client_name = @name", new { name = Name });
            if (client_status == null)
            {
                int rows = connection.Execute("insert into Clients(client_name) values(@name)", new { name = Name });
                return rows > 0 ? Answer.Successfully : Answer.Error;
            }
            else return client_status == Client.Status.AtComputer.ToString() ? Answer.ClientAtComputer : client_status == Client.Status.NotAtComputer.ToString() ? Answer.ClientNotAtComputer : Answer.Error;
        }
    }

    public List<Client> GetAll()
    {
        List<Client> clients = new List<Client>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            List<ClientDTO> clientsDTO = connection.Query<ClientDTO>("select * from Clients").ToList();
            if (clientsDTO.Count > 0) foreach (var client in clientsDTO) clients.Add(new Client(client.client_id, client.client_name, Client.StatusEnum(client.client_status)));
            return clients;
        }
    }

    public Client GetSitterAtComputer(string ComputerName)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string query = $"select cl.* from Sessions s " +
                           $"join Clients cl on s.client_id = cl.client_id " +
                           $"join Computers pc on s.computer_id = pc.computer_id " +
                           $"where pc.computer_name = @computer_name and s.session_ended_at is null";
            var clientDTO = connection.QueryFirstOrDefault<ClientDTO>(query, new { computer_name = ComputerName });
            return clientDTO != null ? new Client(clientDTO.client_id, clientDTO.client_name, Client.StatusEnum(clientDTO.client_status)) : null;
        }
    }
}

class SQLTableSessions : IDBSessions
{
    string connectionString;

    public SQLTableSessions(string ConnectionString)
    {
        connectionString = ConnectionString;
    }

    public Answer Start(int ComputerId, int ClientId)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            SqlTransaction transaction = connection.BeginTransaction();
            try
            {
                string query = "insert into Sessions(computer_id, client_id) Values(@computer_id, @client_id);" +
                               $"update Computers set computer_status = @computer_status where computer_id = @computer_id;" +
                               $"update Clients set client_status = @client_status where client_id = @client_id;";
                connection.Execute(query, new { computer_id = ComputerId, client_id = ClientId, computer_status = Computer.Status.Busy, client_status = Client.Status.AtComputer }, transaction);

                transaction.Commit();
                return Answer.Successfully;
            }
            catch
            {
                transaction.Rollback();
                return Answer.Error;
            }
        }
    }

    public List<Session> GetAll()
    {
        List<Session> sessions = new List<Session>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string query = $"select s.session_id, pc.computer_id, pc.computer_name, pc.computer_status, cl.client_id, cl.client_name, cl.client_status, s.session_started_at, s.session_ended_at from Sessions s " +
                           $"join Computers pc on s.computer_id = pc.computer_id " +
                           $"join Clients cl on s.client_id = cl.client_id ";
            var reader = connection.ExecuteReader(query);
            while (reader.Read())
            {
                sessions.Add(new Session(
                    (int)reader["session_id"],
                    new Computer((int)reader["computer_id"], (string)reader["computer_name"], Computer.StatusEnum((string)reader["computer_status"])),
                    new Client((int)reader["client_id"], (string)reader["client_name"], Client.StatusEnum((string)reader["client_status"])),
                    (DateTimeOffset)reader["session_started_at"],
                    (DateTimeOffset?)reader["session_ended_at"]));
            }
            return sessions;
        }
    }

    public Answer End(int SessionId)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            SessionDTO sessionDTO = connection.QueryFirstOrDefault<SessionDTO>("select * from Sessions where session_id = @session_id and session_ended_at is null", new { session_id = SessionId });
            if (sessionDTO != null)
            {
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    string query = $"update Sessions set session_ended_at = getdate() where session_id = @session_id;" +
                                   $"update Computers set computer_status = @computer_status where computer_id = @computer_id;" +
                                   $"update Clients set client_status = @client_status where client_id = @client_id;";
                    connection.Execute(query, new { session_id = SessionId, computer_id = sessionDTO.computer_id, client_id = sessionDTO.client_id, computer_status = Computer.Status.Free, client_status = Client.Status.NotAtComputer }, transaction);

                    transaction.Commit();
                    return Answer.Successfully;
                }
                catch
                {
                    transaction.Rollback();
                    return Answer.Error;
                }
            }
            else return Answer.NoneSession;
        }
    }
}

interface IDBComputers
{
    Answer Add(string Name);
    Answer Delete(string Name);
    List<Computer> GetAll();
    Computer GetBySittingAtClient(string ClientName);
}

interface IDBClients
{
    Answer Add(string Name);
    List<Client> GetAll();
    Client GetSitterAtComputer(string ComputerName);
}

interface IDBSessions
{
    Answer Start(int ComputerId, int ClientId);
    List<Session> GetAll();
    Answer End(int SessionId);
}