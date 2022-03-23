using System.Text;
using System.Text.Json;
using System.Data;
using System.Security.Cryptography;
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using java.nio.file;

namespace BookManage
{

    public class CSVEXECUTER
    {
        public static class files
        {
            public static string filename = "blank.csv";
        }

        public static void Stuff()
        {   
            //
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document";
            dialog.DefaultExt = ".csv";
            dialog.Filter = "BookData Insertion (.csv)|*.csv";

            bool? result = dialog.ShowDialog();

            string csv_file_path = dialog.FileName;

            //The code that opens the file explorer to select the csv file

            DataTable csvData = CSVreader.GetDataTableFromCSVFile(csv_file_path);
            Console.WriteLine($"Read {csvData.Rows.Count} records");

            List<Book> Books = new List<Book>();

            foreach (DataRow row in csvData.Rows)
            {
                Books.Add(new Book(row[0].ToString(), row[1].ToString(), row[2].ToString(), row[3].ToString(), row[4].ToString()));
            }
            //Console.WriteLine($"Sample - Books 50 was {Books[50]} ");


            SerializeToFile(Books, @"C:/tmp/output.json"); //The code that saves the json after the conversion
        }

        public static void SerializeToFile(object data, string filename)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            using (var stream = File.Create(filename))
            {
                System.Text.Json.JsonSerializer.Serialize(stream, data, options);
                stream.Dispose();
            }
        }
    }



    static class CSVreader //This is to read the csv and put it into a format that the rest of the program can use
    {
        public static DataTable GetDataTableFromCSVFile(string csv_file_path)
        {
            DataTable csvData = new DataTable();
            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return csvData;
        }
    }

    public class Book
    {
        public Book(string name, string title, string publishedIn, string publisher, string date)
        {
            Name = name;
            Title = title;
            PublishedIn = publishedIn;
            Publisher = publisher;
            Date = date;
            Cat = GetCatFor(name, title, publishedIn, publisher, date);
        }
        public string Cat { get; }
        public string Name { get; }
        public string Title { get; }
        public string PublishedIn { get; }
        public string Publisher { get; }
        public string Date { get; }
        private bool PrintMembers(StringBuilder stringBuilder)
        {
            stringBuilder.Append($"Catalogue = {Cat}, Author = {Name}, Title = {Title}, Published in {PublishedIn} by {Publisher}, {Date}");
            return true;
        }
        private static string GetCatFor(string name, string title, string publishedIn, string publisher, string date)
        {
            string source = name + title + publishedIn + publisher + date;
            using (MD5 md5 = MD5.Create())
            {
                string hash = GetHash(md5, source);
                return hash.Substring(0, 10); //This makes the hash only 10 characters long
                //return hash[..10];
            }
        }
        private static string GetHash(HashAlgorithm hashAlgorithm, string input) //The part of the code that generates the unique hash for each book
        {
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}