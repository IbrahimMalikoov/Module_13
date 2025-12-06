using System;

namespace TicketMachineLab
{
    public interface IState
    {
        void SelectTicket(TicketMachine machine, string ticketName, int price);
        void InsertMoney(TicketMachine machine, int amount);
        void DispenseTicket(TicketMachine machine);
        void CancelTransaction(TicketMachine machine);
    }
    public class TicketMachine
    {
        public IState CurrentState { get; set; }
        public string SelectedTicket { get; set; }
        public int TicketPrice { get; set; }
        public int CurrentBalance { get; set; }

        public TicketMachine()
        {
            CurrentState = new IdleState();
            CurrentBalance = 0;
        }
        public void SelectTicket(string ticketName, int price)
        {
            CurrentState.SelectTicket(this, ticketName, price);
        }

        public void InsertMoney(int amount)
        {
            CurrentState.InsertMoney(this, amount);
        }

        public void Dispense()
        {
            CurrentState.DispenseTicket(this);
        }

        public void Cancel()
        {
            CurrentState.CancelTransaction(this);
        }
    }

    public class IdleState : IState
    {
        public void SelectTicket(TicketMachine machine, string ticketName, int price)
        {
            Console.WriteLine($"[Idle] Выбран билет: {ticketName} (Цена: {price})");
            machine.SelectedTicket = ticketName;
            machine.TicketPrice = price;
            machine.CurrentState = new WaitingForMoneyState();
        }

        public void InsertMoney(TicketMachine machine, int amount)
        {
            Console.WriteLine("[Idle] Сначала выберите билет!");
        }

        public void DispenseTicket(TicketMachine machine)
        {
            Console.WriteLine("[Idle] Нечего выдавать.");
        }

        public void CancelTransaction(TicketMachine machine)
        {
            Console.WriteLine("[Idle] Нет активной операции для отмены.");
        }
    }
    public class WaitingForMoneyState : IState
    {
        public void SelectTicket(TicketMachine machine, string ticketName, int price)
        {
            Console.WriteLine("[Оплата] Билет уже выбран. Внесите деньги или нажмите Отмена.");
        }

        public void InsertMoney(TicketMachine machine, int amount)
        {
            machine.CurrentBalance += amount;
            Console.WriteLine($"[Оплата] Внесено: {amount}. Баланс: {machine.CurrentBalance}/{machine.TicketPrice}");

            if (machine.CurrentBalance >= machine.TicketPrice)
            {
                Console.WriteLine("[Оплата] Сумма достаточна.");
                machine.CurrentState = new MoneyReceivedState();
                machine.Dispense();
            }
        }

        public void DispenseTicket(TicketMachine machine)
        {
            Console.WriteLine($"[Оплата] Недостаточно средств. Нужно еще {machine.TicketPrice - machine.CurrentBalance}.");
        }

        public void CancelTransaction(TicketMachine machine)
        {
            Console.WriteLine("[Оплата] Отмена выбора...");
            machine.CurrentState = new TransactionCanceledState();
            machine.Cancel();
        }
    }

    public class MoneyReceivedState : IState
    {
        public void SelectTicket(TicketMachine machine, string ticketName, int price)
        {
            Console.WriteLine("[Готово] Оплата принята. Идет выдача...");
        }

        public void InsertMoney(TicketMachine machine, int amount)
        {
            machine.CurrentBalance += amount;
            Console.WriteLine($"[Готово] Добавлено {amount}. Это пойдет в сдачу.");
        }

        public void DispenseTicket(TicketMachine machine)
        {
            machine.CurrentState = new TicketDispensedState();
            machine.Dispense();
        }

        public void CancelTransaction(TicketMachine machine)
        {
            Console.WriteLine("[Готово] Отмена после оплаты...");
            machine.CurrentState = new TransactionCanceledState();
            machine.Cancel();
        }
    }

    public class TicketDispensedState : IState
    {
        public void SelectTicket(TicketMachine machine, string ticketName, int price) { }
        public void InsertMoney(TicketMachine machine, int amount) { }
        public void CancelTransaction(TicketMachine machine) { }

        public void DispenseTicket(TicketMachine machine)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("==================================");
            Console.WriteLine($"   БИЛЕТ ВЫДАН: {machine.SelectedTicket}");
            Console.WriteLine("==================================");
            Console.ResetColor();

            int change = machine.CurrentBalance - machine.TicketPrice;
            if (change > 0)
            {
                Console.WriteLine($"[Автомат] Ваша сдача: {change}");
            }

            // Сброс параметров
            machine.CurrentBalance = 0;
            machine.SelectedTicket = null;
            machine.TicketPrice = 0;

            // Возврат в начало
            Console.WriteLine("[Автомат] Готов к следующему клиенту.\n");
            machine.CurrentState = new IdleState();
        }
    }
    public class TransactionCanceledState : IState
    {
        public void SelectTicket(TicketMachine machine, string ticketName, int price) { }
        public void InsertMoney(TicketMachine machine, int amount) { }
        public void DispenseTicket(TicketMachine machine) { }

        public void CancelTransaction(TicketMachine machine)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (machine.CurrentBalance > 0)
            {
                Console.WriteLine($"[Возврат] Заберите ваши деньги: {machine.CurrentBalance}");
            }
            Console.WriteLine("[Отмена] Операция отменена.");
            Console.ResetColor();
            machine.CurrentBalance = 0;
            machine.SelectedTicket = null;
            machine.TicketPrice = 0;
            Console.WriteLine("[Автомат] Готов к работе.\n");
            machine.CurrentState = new IdleState();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TicketMachine machine = new TicketMachine();

            Console.WriteLine("--- ТЕСТ 1: Покупка билета ---");
            machine.SelectTicket("Метро", 50);
            machine.InsertMoney(20);
            machine.InsertMoney(30);

            Console.WriteLine("--- ТЕСТ 2: Отмена покупки ---");
            machine.SelectTicket("Автобус", 100);
            machine.InsertMoney(60);
            machine.Cancel();

            Console.ReadKey();
        }
    }
}