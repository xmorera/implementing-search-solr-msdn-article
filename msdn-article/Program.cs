using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSDNArticleDemo.Helpers;
using MSDNArticleDemo.Indexer;
using MSDNArticleDemo.Searcher;

namespace MSDNArticleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckNoArgumentsOrInvalid(args);

            //Initialize SolrNet
            HelperFunctions helpers = new HelperFunctions();
            helpers.InitializeSolrNet();

            //Start the requested option
            int user_option = Int32.Parse(args[0]); 
            switch (user_option)
            {
                case 0:
                    DeleteAll(helpers);
                    break;
                case 1:
                    IndexData();
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                    SearchData(user_option);
                    break;
                default:
                    Console.WriteLine("Invalid option");
                    break;
            }
        }

        /// <summary>
        /// Checks that correct arguments are provided
        /// </summary>
        /// <param name="args">Command line arguments</param>
        private static void CheckNoArgumentsOrInvalid(string[] args)
        {
            if ((args.Length == 0) || !(new[] {"0", "1", "2", "3", "4", "5"}.Contains(args[0])))
            {
                Console.WriteLine("Please provide a valid argument:");
                Console.WriteLine("  'msdn-article-demo 0' for deleting all documents");
                Console.WriteLine("  'msdn-article-demo 1' for indexing all documents");
                Console.WriteLine("  'msdn-article-demo 2' for searching all documents by keyword");
                Console.WriteLine("  'msdn-article-demo 3' for searching all questions by keyword");
                Console.WriteLine("  'msdn-article-demo 4' for getting all tags for a keyword search");
                Console.WriteLine("  'msdn-article-demo 5' for searching all questions by a tag");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Deletes all documents in the index
        /// </summary>
        private static void DeleteAll(HelperFunctions helpers)
        {
            try
            {
                helpers.ClearIndex();
            }
            catch (Exception ex)
            {
                Console.Write("Error deleting documents: " + ex.Message);
            }
        }

        /// <summary>
        /// Starts indexing process
        /// </summary>
        private static void IndexData()
        {
            PostIndexer indexer = new PostIndexer();
            try
            {
                indexer.IndexDocuments();
            }
            catch (Exception ex)
            {
                Console.Write("Error indexing documents: " + ex.Message);
            }
        }

        /// <summary>
        /// Starts a small search server
        /// </summary>
        private static void SearchData(int which_option)
        {
            PostSearcher searcher = new PostSearcher();
            try
            {
                searcher.SearchDocuments(which_option);
            }
            catch (Exception ex)
            {
                Console.Write("Error executing search: " + ex.Message);
            }
        }
    }
}
