using System;
using System.Collections.Generic;

namespace ConsoleApp3
{
    public interface ICurrencyConverter
    {
        /// Clears any prior configuration.
        void ClearConfiguration();

        /// Updates the configuration. Rates are inserted or replaced internally.
        void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates);

        /// Converts the specified amount to the desired currency.
        double Convert(string fromCurrency, string toCurrency, double amount);
    }
    public sealed class CurrencyConverter : ICurrencyConverter
    {
        public int defaultConfigurationCount;
        string path = "";
        private List<Tuple<string, double>> conversionRatesList = new List<Tuple<string, double>>();
        private List<Tuple<string, int, double>> conversionPathList = new List<Tuple<string, int, double>>();
        private List<string> currencyList = new List<string>();

        private CurrencyConverter() { }
        private static CurrencyConverter instance;



        public static CurrencyConverter GetInstance()
        {
            if(instance == null)
            {
                instance = new CurrencyConverter();
                instance.fillList();
                instance.defaultConfigurationCount = instance.conversionRatesList.Count;
            }
            return instance;

        }
        public List<Tuple<string, double>> getList()
        {
            return conversionRatesList;
        }

        private void fillList()
        {
            //conversionRatesList.Add(new Tuple<string, double>("USD-CAD", 1.34));
            //conversionRatesList.Add(new Tuple<string, double>("CAD-GBP", 0.58));
            //conversionRatesList.Add(new Tuple<string, double>("USD-EUR", 0.86));
            conversionRatesList.Add(new Tuple<string, double>("A-M", 1.3));
            conversionRatesList.Add(new Tuple<string, double>("M-L", 0.5));
            conversionRatesList.Add(new Tuple<string, double>("A-D", 0.6));
            conversionRatesList.Add(new Tuple<string, double>("D-B", 0.4));
            conversionRatesList.Add(new Tuple<string, double>("B-C", 1.9));
            conversionRatesList.Add(new Tuple<string, double>("B-F", 0.8));
            conversionRatesList.Add(new Tuple<string, double>("C-E", 0.65));
            conversionRatesList.Add(new Tuple<string, double>("F-R", 2.58));
            conversionRatesList.Add(new Tuple<string, double>("F-Z", 1.18));
            conversionRatesList.Add(new Tuple<string, double>("E-R", 0.88));
            conversionRatesList.Add(new Tuple<string, double>("R-G", 0.81));
            printCurrencyList(conversionRatesList);
        }
        public int checkInput(string input)
        {
            return currencyList.IndexOf(input);
        }
        private void printCurrencyList(List<Tuple<string, double>> conversionRatesList)
        {
            foreach (Tuple<string, double> config in conversionRatesList)
            {
                string path = config.Item1;
                string[] a = path.Split("-");
                foreach (string v in a) if (currencyList.IndexOf(v) == -1) currencyList.Add(v);
            }

            Console.WriteLine("List of currencies:");
            Console.WriteLine("\t"+ string.Join(",", currencyList));
        }
        public void ClearConfiguration()
        {
            for (int i = defaultConfigurationCount; i < conversionRatesList.Count; i++)
            {
                conversionRatesList.RemoveAt(i);
            }
        }
        // OK
        public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> conversionRates)
        {
            foreach (Tuple<string, string, double> config in conversionRates)
            {
                Tuple<string, double> newConfig = new Tuple<string, double>(config.Item1 + "-" + config.Item2, config.Item3);
                int index = conversionRatesList.FindIndex(a => a.Item1.Equals(newConfig.Item1));
                if (index != -1)
                {
                    conversionRatesList.RemoveAt(index);
                }
                conversionRatesList.Add(newConfig);
            }
        }
        // OK
        public double Convert(string fromCurrency, string toCurrency, double amount)
        {
            double conversionRate = conversionPath(fromCurrency, toCurrency);
            return amount * conversionRate;
        }
        private double conversionPath(string fromCurrency, string toCurrency)
        {
            List<Tuple<string, double>> convList = getList();

            string node = fromCurrency;
            path = fromCurrency;
            string preNode = "NULL";
            int weight = 0;
            double exchangeRate = 1;

            conversionPathList.Clear();
            // look for toCurrency
            iterate(convList, fromCurrency, toCurrency, preNode, node, weight, exchangeRate);
            // print path list
            foreach (Tuple<string, int, double> path in conversionPathList)
            {
                Console.WriteLine("path: {0}, weight: {1}, exchangeRate: {2}", path.Item1, path.Item2, path.Item3);
            }

            // find shortest path
            int w = conversionPathList[0].Item2;
            double exRate = conversionPathList[0].Item3;
            foreach (Tuple<string, int, double> path in conversionPathList) 
                if (path.Item2 < w) exRate = path.Item3;
            return exRate;

        }
        private int checkWholeVertex(List<Tuple<string, double>> new_cl, string endPoint)
        {
            int found = new_cl.FindIndex(a => a.Item1.Contains(endPoint));
            return found;
        }
        private void foundEndPoint(Tuple<string, double> cl, string endPoint, string node, int weight, double exchangeRate)
        {
            string fromTo = cl.Item1;
            double rate = cl.Item2;
            path += "-" + endPoint;
            string[] a = fromTo.Split("-");
            if (a[0].Equals(endPoint))
            {
                exchangeRate *= 1 / rate;
            } else
            {
                exchangeRate *= rate;
            }

            // store path, weight and rate to a list;
            conversionPathList.Add(new Tuple<string, int, double>(path, weight, exchangeRate));
        }
        private void iterate(List<Tuple<string, double>> convList, string startPoint, string endPoint, string preNode, string node, int weight, double exchangeRate)
        {
            weight++;
            List<Tuple<string, double>> new_cl = convList
                .FindAll(a => a.Item1.Contains(node) & !a.Item1.Contains(preNode));

            int found = checkWholeVertex(new_cl, endPoint);
            if (found != -1)
            {
                foundEndPoint(new_cl[found], endPoint, node, weight, exchangeRate);
            } else
            {
                foreach (Tuple<string, double> config in new_cl)
                {
                    string fromTo = config.Item1;
                    double rate = config.Item2;

                    string nextNode = fromTo.Replace(node, "").Replace("-", "");
                    if (nextNode.Length == 2)
                    {
                        // another pass has been started
                        int indexx = path.IndexOf(preNode);
                        path = path.Substring(0, indexx + 1);
                        foreach (char c in path)
                        {
                            int index = nextNode.IndexOf(c);
                            if (index != -1) nextNode = nextNode.Remove(index, 1);
                        }
                    }
                    else
                    {
                        preNode = node;
                    }
                    if (!path.Contains(nextNode))
                    {
                        path += "-" + nextNode;
                        // begin new route
                        node = nextNode;
                        iterate(convList, startPoint, endPoint, preNode, node, weight, exchangeRate);
                    }
                    
                }
            }
            
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Currency Converter Module!");
            CurrencyConverter cc = CurrencyConverter.GetInstance();
            bool again = true;

            while (again)
            {
                Console.Write("Enter from currency: ");
                string from = Console.ReadLine();

                while (cc.checkInput(from) == -1)
                {
                    Console.WriteLine("Not a valid currency, try again:");
                    from = Console.ReadLine();
                }
                Console.Write("Enter to currency: ");
                string to = Console.ReadLine();

                while (cc.checkInput(to) == -1)
                {
                    Console.WriteLine("Not a valid currency, try again:");
                    to = Console.ReadLine();
                }

                Console.Write("Enter price: ");
                
                double X;
                String Result = Console.ReadLine();

                while (!Double.TryParse(Result, out X))
                {
                    Console.WriteLine("Not a valid number, try again:");

                    Result = Console.ReadLine();
                }
                double price = Convert.ToDouble(Result);
                Console.WriteLine("calculate currency from: {0} to: {1} with the price: {2}", from, to, price);
                
                Console.WriteLine("converted price is: {0}", cc.Convert(from, to, price).ToString());

                Console.Write("Again(Y/N): ");
                if (Console.ReadLine() == "N") again = false;

            }

            Console.Write("Good Luck!");

            Console.ReadLine();

        }
    }
}
