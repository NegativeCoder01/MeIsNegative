using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace breadOS
{
    public class Program
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static Dictionary<string, string> LoadConfig(string path)
        {
            var config = new Dictionary<string, string>();
            if (!File.Exists(path))
                return config;

            foreach (var line in File.ReadAllLines(path))
            {
                if (line.Contains('='))
                {
                    var parts = line.Split('=', 2);
                    config[parts[0].Trim()] = parts[1].Trim();
                }
            }
            return config;
        }

        public static void SaveConfig(string path, Dictionary<string, string> config)
        {
            using (var writer = new StreamWriter(path))
            {
                foreach (var kv in config)
                {
                    writer.WriteLine($"{kv.Key}={kv.Value}");
                }
            }
        }

        public static void LogCommand(string path, string command)
        {
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine($"{DateTime.Now}: {command}");
            }
        }

        public static string ReadCommandWithHistory(List<string> history)
        {
            StringBuilder input = new StringBuilder();
            int historyIndex = history.Count;
            int cursorLeft = Console.CursorLeft;
            int cursorTop = Console.CursorTop;

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input.ToString();
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input.Remove(input.Length - 1, 1);
                        Console.SetCursorPosition(cursorLeft, cursorTop);
                        Console.Write(new string(' ', Console.WindowWidth - cursorLeft));
                        Console.SetCursorPosition(cursorLeft, cursorTop);
                        Console.Write(input.ToString());
                    }
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    if (history.Count > 0 && historyIndex > 0)
                    {
                        historyIndex--;
                        input.Clear();
                        input.Append(history[historyIndex]);
                        Console.SetCursorPosition(cursorLeft, cursorTop);
                        Console.Write(new string(' ', Console.WindowWidth - cursorLeft));
                        Console.SetCursorPosition(cursorLeft, cursorTop);
                        Console.Write(input.ToString());
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (history.Count > 0 && historyIndex < history.Count - 1)
                    {
                        historyIndex++;
                        input.Clear();
                        input.Append(history[historyIndex]);
                        Console.SetCursorPosition(cursorLeft, cursorTop);
                        Console.Write(new string(' ', Console.WindowWidth - cursorLeft));
                        Console.SetCursorPosition(cursorLeft, cursorTop);
                        Console.Write(input.ToString());
                    }
                    else if (historyIndex == history.Count - 1)
                    {
                        historyIndex++;
                        input.Clear();
                        Console.SetCursorPosition(cursorLeft, cursorTop);
                        Console.Write(new string(' ', Console.WindowWidth - cursorLeft));
                        Console.SetCursorPosition(cursorLeft, cursorTop);
                    }
                }
                else
                {
                    input.Append(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
            }
        }

        public static void Main()
        {
            string configPath = "/workspaces/MeIsNegative/Csharp/Computer/FakeSmallOS/data.conf"; //enter correct path here to run, I am in codespace so this path works for me
            string historyPath = "/workspaces/MeIsNegative/Csharp/Computer/FakeSmallOS/history.txt"; //same here
            var config = LoadConfig(configPath);

            List<string> commandHistory = new List<string>();
            if (File.Exists(historyPath))
                commandHistory.AddRange(File.ReadAllLines(historyPath));

            bool user_Exist = config.ContainsKey("user_Exist") && config["user_Exist"] == "true";
            bool user_Logged = false;
            string username = config.ContainsKey("username") ? config["username"] : "";
            string passwordHash = config.ContainsKey("password") ? config["password"] : "";

            while (true)
            {
                if (!user_Exist)
                {
                    Console.WriteLine("Create new user...");
                    Console.Write("Enter new username: ");
                    username = Console.ReadLine();

                    Console.Write("Enter your new password: ");
                    string newPassword = Console.ReadLine();
                    passwordHash = HashPassword(newPassword);

                    Console.WriteLine("New user created...");
                    user_Exist = true;

                    config["username"] = username;
                    config["password"] = passwordHash;
                    config["user_Exist"] = "true";
                    SaveConfig(configPath, config);
                }
                else
                {
                    if (!user_Logged)
                    {
                        Console.WriteLine("Login");
                        Console.Write("Enter your username: ");
                        string usernameLoginInput = Console.ReadLine();

                        Console.Write("Enter your password: ");
                        string passwordLoginInput = Console.ReadLine();
                        string hashedInput = HashPassword(passwordLoginInput);

                        if (usernameLoginInput == username && hashedInput == passwordHash)
                        {
                            Console.WriteLine("Logged in successfully...");
                            user_Logged = true;
                        }
                        else
                        {
                            Console.WriteLine("INCORRECT USERNAME OR PASSWORD!");
                        }
                    }
                    else
                    {
                        Console.Write("Type your command (Type --help for help): ");
                        string commandInput = ReadCommandWithHistory(commandHistory);

                        commandHistory.Add(commandInput);
                        LogCommand(historyPath, commandInput);

                        switch (commandInput)
                        {
                            case "--help":
                                Console.WriteLine("");
                                Console.WriteLine("Help: ");
                                Console.WriteLine(" ");
                                Console.WriteLine("User commands: ");
                                Console.WriteLine("user --del user[]: Deletes user");
                                Console.WriteLine("user --change password : changes password");
                                Console.WriteLine("user --change username : Changes username");
                                Console.WriteLine("user --logout : logs out the user");
                                Console.WriteLine("user --history.clear : clears your command history");
                                Console.WriteLine("");
                                Console.WriteLine("System commands: ");
                                Console.WriteLine("sys --reboot : Reboots the OS");
                                Console.WriteLine("sys --reboot.wipe : Reboots and wipes everything");
                                Console.WriteLine("sys --exit : exits the system");
                                Console.WriteLine("");
                                Console.WriteLine("Application commands: ");
                                Console.WriteLine("app --install.help : shows you avaliable packages");
                                Console.WriteLine("app --install [package name] : installs the application based off the package");
                                Console.WriteLine("");
                                Console.WriteLine("Other: ");
                                Console.WriteLine("clear : clears the terminal");
                                break;
                            case "user --del user[]":
                                Console.Write("Enter your username: ");
                                string usernameDelInput = Console.ReadLine();

                                Console.Write("Enter your password: ");
                                string passwordDelInput = Console.ReadLine();
                                string hashedDelInput = HashPassword(passwordDelInput);

                                if (usernameDelInput == username && hashedDelInput == passwordHash)
                                {
                                    Console.Write($"Are you sure you want to delete the user \"{username}\"? N/y: ");
                                    string userDelConfirmInput = Console.ReadLine();

                                    if (userDelConfirmInput == "y")
                                    {
                                        Console.WriteLine("User deletion is a success");
                                        Console.WriteLine("Goodbye...");
                                        user_Exist = false;
                                        user_Logged = false;
                                        username = "";
                                        passwordHash = "";

                                        config["user_Exist"] = "false";
                                        config["username"] = "";
                                        config["password"] = "";
                                        SaveConfig(configPath, config);
                                    }
                                    else
                                    {
                                        Console.WriteLine("User deletion cancelled...");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("INCORRECT USERNAME OR PASSWORD!");
                                }
                                break;
                            case "user --change password":
                                Console.Write("Enter your username: ");
                                string usernameChangePassInput = Console.ReadLine();

                                Console.Write("Enter your password: ");
                                string passwordChangePassInput = Console.ReadLine();
                                string hashedChangePassInput = HashPassword(passwordChangePassInput);

                                if (usernameChangePassInput == username && hashedChangePassInput == passwordHash)
                                {
                                    Console.Write("Enter new password: ");
                                    string newPassword = Console.ReadLine();

                                    passwordHash = HashPassword(newPassword);
                                    config["password"] = passwordHash;
                                    SaveConfig(configPath, config);
                                }
                                else
                                {
                                    Console.WriteLine("INCORRECT USERNAME OR PASSWORD!");
                                }
                                break;
                            case "user --change username":
                                Console.Write("Enter your username: ");
                                string usernameChangeNameInput = Console.ReadLine();

                                Console.Write("Enter your password: ");
                                string passwordChangeNameInput = Console.ReadLine();
                                string hashedChangeNamePassInput = HashPassword(passwordChangeNameInput);

                                if (usernameChangeNameInput == username && hashedChangeNamePassInput == passwordHash)
                                {
                                    Console.Write("Enter new username: ");
                                    string newUsername = Console.ReadLine();

                                    username = newUsername;
                                    config["username"] = username;
                                    SaveConfig(configPath, config);
                                }
                                else
                                {
                                    Console.WriteLine("INCORRECT USERNAME OR PASSWORD!");
                                }
                                break;
                            case "user --logout":
                                Console.WriteLine("Goodbye...");
                                user_Logged = false;
                                break;
                            case "sys --reboot":
                                Console.WriteLine("Rebooting...");
                                user_Logged = false;
                                break;
                            case "sys --reboot.wipe":
                                Console.Write("Enter your username: ");
                                string usernameSysWipe = Console.ReadLine();

                                Console.Write("Enter your password: ");
                                string passwordSysWipe = Console.ReadLine();
                                string hashedSysWipePass = HashPassword(passwordSysWipe);

                                if (usernameSysWipe == username && hashedSysWipePass == passwordHash)
                                {
                                    Console.Write("Are you sure you want to wipe the system? N/y: ");
                                    string wipeConfirmation = Console.ReadLine();

                                    if (wipeConfirmation == "y")
                                    {
                                        Console.WriteLine("Wiping system...");
                                        user_Exist = false;
                                        user_Logged = false;
                                        username = "";
                                        passwordHash = "";

                                        config["user_Exist"] = "false";
                                        config["username"] = "";
                                        config["password"] = "";
                                        config["randomjokeInstalled"] = "false";
                                        config["numbercomboInstalled"] = "false";
                                        SaveConfig(configPath, config);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Wipe cancelled...");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("INCORRECT USERNAME OR PASSWORD!");
                                }
                                break;
                            case "app --install.help":
                                Console.WriteLine("");
                                Console.WriteLine("Availiable packages: ");
                                Console.WriteLine("app.randomjoke.bap");
                                Console.WriteLine("app.numbercombo.bap");
                                Console.WriteLine("");
                                Console.WriteLine("BAP stands for Bread application Packager");
                                break;
                            case "app --install app.randomjoke.bap":
                                if (config.ContainsKey("randomjokeInstalled") && config["randomjokeInstalled"] == "true")
                                {
                                    Console.WriteLine("Package is already installed!");
                                    Console.WriteLine("Run \"randomjoke\" to run the program.");
                                }
                                else
                                {
                                    Console.WriteLine("[installing packages...]");
                                    Thread.Sleep(850);
                                    Console.WriteLine("Package installed!");
                                    Console.WriteLine("run \"randomjoke\" to start the application!");
                                    config["randomjokeInstalled"] = "true";
                                    SaveConfig(configPath, config);
                                }
                                break;
                            case "randomjoke":
                                if (config.ContainsKey("randomjokeInstalled") && config["randomjokeInstalled"] == "true")
                                {
                                    string[] jokes = {
                                        "Knock knock, who's there, your dad, my dad who? ....",
                                        "Why did the chicken want to cross the road... because it was.",
                                        "Before crow bars were invented, crows drank at home."
                                    };
                                    Random r = new Random();
                                    Console.WriteLine(jokes[r.Next(jokes.Length)]);
                                }

                                else
                                {
                                    Console.WriteLine("Package not installed. Run app --install app.randomjoke.bap to install it.");
                                }
                                break;
                            case "user --history.clear":
                                File.WriteAllText(historyPath, string.Empty);
                                Console.WriteLine("History cleared, next time you reboot your system past commands will be gone...");
                                break;
                            case "app --install app.numbercombo.bap":
                                if (config.ContainsKey("numbercomboInstalled") && config["numbercomboInstalled"] == "true")
                                {
                                    Console.WriteLine("Package is already installed...");
                                }
                                else
                                {
                                    Console.WriteLine("[Installing packages...]");
                                    Thread.Sleep(883);
                                    Console.WriteLine("Packages installed!");
                                    Console.WriteLine("Type \"numbercombo\" to run it");
                                    config["numbercomboInstalled"] = "true";
                                }
                                break;
                            case "numbercombo":
                                if (config.ContainsKey("numbercomboInstalled") && config["numbercomboInstalled"] == "true")
                                {
                                    Console.Write("Enter the number of digits in the combination (1 to 18): ");
                                    if (!int.TryParse(Console.ReadLine(), out int digits) || digits < 1 || digits > 18)
                                    {
                                        Console.WriteLine("Invalid input. Please enter a number between 1 and 18.");
                                        return;
                                    }

                                    long max = (long)Math.Pow(10, digits);

                                    for (long i = 0; i < max; i++)
                                    {
                                        Console.WriteLine(i.ToString().PadLeft(digits, '0'));
                                    }

                                    Console.WriteLine("Done! Press any key to exit.");
                                    Console.ReadKey();
                                }
                                else
                                {
                                    Console.Write("package is not installed, would you like to install it? N/y: ");
                                    string numbercomboInstallConfirmation = Console.ReadLine();

                                    if (numbercomboInstallConfirmation == "y")
                                    {
                                        Console.WriteLine("[installing packages...]");
                                        Thread.Sleep(1476);
                                        Console.WriteLine("Packages installed!");
                                        Console.WriteLine(" Type \"numbercombo\" to run it");
                                        config["numbercomboInstalled"] = "true";
                                    }
                                    else
                                    {
                                        Console.WriteLine("Installation cancled...");
                                    }
                                }
                                break;
                            case "app --remove app.numbercombo.bap":
                                Console.WriteLine("Removed numbercombo!");
                                config["numbercomboInstalled"] = "false";
                                break;
                            case "app --remove app.randomjoke.bap":
                                Console.WriteLine("Removed randomjoke!");
                                config["randomjokeInstalled"] = "false";
                                break;
                            case "bruh":
                                Console.WriteLine("⢸⣿⠛⠛⠿⣷⣄⠀⠀⠀⢰⡿⠟⠛⠯⢭⣦⡄⠀⠀⠀⠀⠀⢠⡄⠀⠀⠀⠀⠀⠀⢠⡄⠀⢠⡄⠀⠀⠀⢠⡄");
                                Console.WriteLine("⣿⠀⠀⠀⠀⠀⢹⣷⠀⠀⢸⡇⠀⢀⢀⣎⡼⠁⠀⠀⠀⠀⠀⢸⡇⠀⠀⠀⠀⠀⠀⢸⡇⠀⢸⡇⠀⠀⠀⢸⡇");
                                Console.WriteLine("⣻⠃⠀⣤⣤⣤⣼⠇⠀⠀⢸⡏⠉⠛⢯⡀⠀⠀⠀⠀⠀⠀⠀⢸⡇⠀⠀⠀⠀⠀⠀⢸⡇⠀⢸⣧⣤⣤⣤⣼⡇");
                                Console.WriteLine("⢸⡇⠀⠛⠛⠛⠓⡄⠀⠀⢸⡇⠀⠀⠀⠈⣇⠀⠀⠀⠀⠀⠀⢸⡇⠀⠀⠀⠀⠀⠀⢸⡇⠀⢸⡏⠉⠉⠉⢹⡇");
                                Console.WriteLine("⢸⡇⠀⠀⠀⠀⠀⡇⠀⠀⢸⡇⠀⠀⠀⠀⠀⢋⡄⠀⠀⠀⠀⠀⣧⡀⠀⠀⠀⠀⢀⣼⡇⠀⢸⡇⠀⠀⠀⢸⡇");
                                Console.WriteLine("⢿⣧⣤⡶⠶⠚⠋⠁⠀⠀⠸⠇⠀⠀⠀⠀⠀⠀⠈⠓⠀⠀⠀⠀⠈⠿⣷⣤⣤⣾⠿⠁⠀⠀⢸⡇⠀⠀⠀⢸⡇");
                                break;
                            case "sys --exit":
                                Environment.Exit(0);
                                break;
                            case "clear":
                                Console.Clear();
                                break;



                        }
                    }
                }
            }
        }
    }
}
