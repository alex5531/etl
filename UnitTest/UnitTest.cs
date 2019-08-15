using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using etl.Impl;

namespace UnitTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void DestinationCheckHeaders()
        {
            var dest = new Destination(new List<string> { "AccountCode", "Name", "Type", "Open Date", "Currency" })
                .WithHeader(true)
                .WithAction((x) => Assert.AreEqual("AccountCode,Name,Type,Open Date,Currency", x));
            var src = new Source(new List<string> { "Name" }, new List<string>());
            var t = new Transf(src, dest)
                .Map("Name")
                .Execute();
        }

        [TestMethod]
        [ExpectedException(typeof(AssertFailedException))]
        public void DestinationCheckHeadersExeption()
        {
            var dest = new Destination(new List<string> { "AccountCode", "Name", "Type", "Open Date", "Currency" })
                .WithHeader(true)
                .WithAction((x) => Assert.AreEqual("AccountCode,Name,Type,Open Date,Currency1", x));
            var src = new Source(new List<string> { "Name" }, new List<string>());
            var t = new Transf(src, dest)
                .Map("Name")
                .Execute();
        }

        [TestMethod]
        public void DestinationHeaderTest()
        {
            var dest = new Destination(new List<string> { "AccountCode", "Name", "Type", "Open Date", "Currency" })
                .WithHeader(false)
                .WithAction((x) => Assert.AreEqual(string.Empty, x));
            var src = new Source(new List<string> { "Name" }, new List<string>());
            var t = new Transf(src, dest)
                .Map("Name")
                .Execute();
        }

        [TestMethod]
        public void SourceHeaderTest()
        {
            var src = new Source(new List<string> { "Name", "Type", "Currency", "Custodian Code" }, new List<string>());
            Assert.AreEqual("Name,Type,Currency,Custodian Code", string.Join(",", new List<string>(src.GetFieldNames()).ToArray()));            
        }

        [TestMethod]
        public void CheckSrcData()
        {
            var src = new Source(
               new List<string> { "Identifier", "Name", "Type", "Opened", "Currency" },
               new List<string> { "123|AbcCode", "My Account", "2", "01-01-2018", "CD" });
            var data = new List<Dictionary<string, object>>(src.Rows)[0];
            data.TryGetValue("Identifier", out object val);
            Assert.AreEqual("123|AbcCode", val);
            data.TryGetValue("Name", out val);
            Assert.AreEqual("My Account", val);
            data.TryGetValue("Type", out val);
            Assert.AreEqual("2", val);
            data.TryGetValue("Opened", out val);
            Assert.AreEqual("01-01-2018", val);
            data.TryGetValue("Currency", out val);
            Assert.AreEqual("CD", val);
        }

        [TestMethod]
        public void FirstCaseTest()
        {
            var result = new List<string>();
            var dest = new Destination(new List<string> { "AccountCode", "Name", "Type", "Open Date", "Currency" })
               .WithHeader(true)
               .WithAction((x) => result.Add(x));

            var src = new Source(
                new List<string> { "Identifier", "Name", "Type", "Opened", "Currency" },
                new List<string> { "123|AbcCode", "My Account", "2", "01-01-2018", "CD" });

            var t = new Transf(src, dest)
                .Map("Identifier", "AccountCode", acc => GetAccount((string)acc))
                .Map("Name")
                .Map("Type", "Type", type => GetAccType((string)type))
                .Map("Opened", "Open Date")
                .Map<string>("Currency", "Currency", cur => GetCurrency(cur))
                .Execute();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("AccountCode,Name,Type,Open Date,Currency", result[0]);
            Assert.AreEqual("AbcCode,My Account,RRSP,01-01-2018,CAD", result[1]);
        }

        [TestMethod]
        public void SecondCaseTest()
        {
            var result = new List<string>();
            var dest = new Destination(new List<string> { "AccountCode", "Name", "Type", "Open Date", "Currency" })
                .WithHeader(true)
                .WithAction((x) => result.Add(x));

            var src = new Source(
                new List<string> { "Name", "Type", "Currency", "Custodian Code" },
                new List<string> { "My Account", "RRSP", "C", "" });

            var t = new Transf(src, dest)
                .Map("Name")
                .Map("Custodian Code", "AccountCode")
                .Map("Type")
                .Map<string>("Currency", "Currency", cur => cur == "C" ? "CAD" : "USD")
                .Execute();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("AccountCode,Name,Type,Open Date,Currency", result[0]);
            Assert.AreEqual(",My Account,RRSP,,CAD", result[1]);
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
            switch (curr)
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
