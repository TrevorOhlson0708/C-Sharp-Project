using System;
using System.Collections.Generic;

class Account
{
    public int Id { get; }
    public string Name { get; }
    public decimal Balance { get; private set; }

    public Account(int id, string name, decimal balance)
    {
        Id = id;
        Name = name;
        Balance = balance;
    }

    public void Deposit(decimal amount)
    {
        if (amount > 0)
            Balance += amount;
    }

    public bool Withdraw(decimal amount)
    {
        if (amount > 0 && amount <= Balance)
        {
            Balance -= amount;
            return true;
        }
        return false;
    }
}

class Bank
{
    private Dictionary<int, Account> accounts = new Dictionary<int, Account>();
    private int nextId = 1;

    public Account CreateAccount(string name, decimal initialBalance)
    {
        Account account = new Account(nextId, name, initialBalance);
        accounts.Add(nextId, account);
        nextId++;
        return account;
    }

    public Account GetAccount(int id)
    {
        return accounts.ContainsKey(id) ? accounts[id] : null;
    }

    public bool Transfer(int fromId, int toId, decimal amount)
    {
        Account from = GetAccount(fromId);
        Account to = GetAccount(toId);

        if (from == null || to == null)
            return false;

        if (from.Withdraw(amount))
        {
            to.Deposit(amount);
            return true;
        }
        return false;
    }

    public void DisplayAccounts()
    {
        foreach (var account in accounts.Values)
        {
            Console.WriteLine($"ID: {account.Id} | Name: {account.Name} | Balance: ${account.Balance}");
        }
    }
}

class Program
{
    static void Main()
    {
        Bank bank = new Bank();
        bool running = true;

        while (running)
        {
            Console.WriteLine("\n1. Create Account");
            Console.WriteLine("2. Deposit");
            Console.WriteLine("3. Withdraw");
            Console.WriteLine("4. Transfer");
            Console.WriteLine("5. View Accounts");
            Console.WriteLine("6. Exit");
            Console.Write("Choose an option: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Name: ");
                    string name = Console.ReadLine();
                    Console.Write("Initial Balance: ");
                    decimal initBalance = decimal.Parse(Console.ReadLine());
                    Account newAccount = bank.CreateAccount(name, initBalance);
                    Console.WriteLine($"Account created with ID {newAccount.Id}");
                    break;

                case "2":
                    Console.Write("Account ID: ");
                    int depId = int.Parse(Console.ReadLine());
                    Console.Write("Amount: ");
                    decimal depAmount = decimal.Parse(Console.ReadLine());
                    Account depAccount = bank.GetAccount(depId);
                    if (depAccount != null)
                    {
                        depAccount.Deposit(depAmount);
                        Console.WriteLine("Deposit successful");
                    }
                    else
                        Console.WriteLine("Account not found");
                    break;

                case "3":
                    Console.Write("Account ID: ");
                    int withId = int.Parse(Console.ReadLine());
                    Console.Write("Amount: ");
                    decimal withAmount = decimal.Parse(Console.ReadLine());
                    Account withAccount = bank.GetAccount(withId);
                    if (withAccount != null && withAccount.Withdraw(withAmount))
                        Console.WriteLine("Withdrawal successful");
                    else
                        Console.WriteLine("Withdrawal failed");
                    break;

                case "4":
                    Console.Write("From Account ID: ");
                    int fromId = int.Parse(Console.ReadLine());
                    Console.Write("To Account ID: ");
                    int toId = int.Parse(Console.ReadLine());
                    Console.Write("Amount: ");
                    decimal transAmount = decimal.Parse(Console.ReadLine());
                    if (bank.Transfer(fromId, toId, transAmount))
                        Console.WriteLine("Transfer successful");
                    else
                        Console.WriteLine("Transfer failed");
                    break;

                case "5":
                    bank.DisplayAccounts();
                    break;

                case "6":
                    running = false;
                    break;

                default:
                    Console.WriteLine("Invalid choice");
                    break;
            }
        }
    }
}
