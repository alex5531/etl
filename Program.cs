using etl.Impl;
using System;
using System.Collections.Generic;


namespace etl
{
    class Program
    {
        static void Main(string[] args)
        {
            var dest = new Destination(new List<string> { "AccountCode", "Name", "Type", "Open Date", "Currency" })
                .WithHeader(true)
                .WithAction((x) => Console.WriteLine(x));

            var src = new Source(
                new List<string> { "Identifier", "Name", "Type", "Opened", "Currency"},
                new List<string> { "123|AbcCode", "My Account", "2", "01-01-2018", "CD"});

            var t = new Transf(src, dest)
                .Map("Identifier", "AccountCode", acc => GetAccount((string)acc))
                .Map("Name")
                .Map("Type", "Type", type => GetAccType((string)type))
                .Map("Opened", "Open Date")
                .Map<string>("Currency", "Currency", cur => GetCurrency(cur))
                .Execute();

            Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();

            dest = new Destination(new List<string> { "AccountCode", "Name", "Type", "Open Date", "Currency" })
                .WithHeader(true)
                .WithAction((x) => Console.WriteLine(x));

            src = new Source(
                new List<string> { "Name", "Type", "Currency", "Custodian Code" },
                new List<string> { "My Account", "RRSP", "C", "" });

            t = new Transf(src, dest)
                .Map("Name")
                .Map("Custodian Code", "AccountCode")
                .Map("Type")
                .Map<string>("Currency", "Currency", cur => cur == "C" ? "CAD" : "USD")
                .Execute();

            Console.ReadKey();

        }

        private static string GetAccType(string type)
        {
            switch (type)
            {
                case "1":
                    return "Trading";
                case "2":
                    return "RRSP";
                case "3":
                    return "RESP";
                case "4":
                    return "Fund";
                default:
                    throw new NotSupportedException();
            }
        }

        private static string GetAccount(string acc)
        {
            var ind = acc.IndexOf("|");
            return ind < 0 ? acc : acc.Substring(ind + 1, acc.Length - ind - 1);
        }

        private static string GetCurrency(string curr)
        {
            switch(curr)
            {
                case "CD":
                    return "CAD";
                case "US":
                    return "USD";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
