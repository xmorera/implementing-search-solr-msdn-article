using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;
using MSDNArticleDemo.Models;
using SolrNet;
using SolrNet.Commands.Parameters;

namespace MSDNArticleDemo.Searcher
{
    class PostSearcher
    {
        public void SearchDocuments(int which_search)
        {
            Console.WriteLine("Searching initialized");

            while (true) // Request search terms, one loop at a time
            {
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Please enter the text you want to search for: (Or just hit enter to exit)");

                string keywords = Console.ReadLine();
                if (string.IsNullOrEmpty(keywords))
                {
                    Console.WriteLine("Thank you for searching!");
                    break;
                }

                // Get the tag, only used in SearchQuestionsByTag
                string tag = string.Empty;
                if (which_search == 5)
                { 
                Console.WriteLine("Please specify which tag: ");
                tag = Console.ReadLine();
                Console.Write(Environment.NewLine);
                }

                // Different searchs method to explore querying with different parameters
                switch (which_search)
                {
                    case 2:
                        SearchBasic(keywords);
                        break;
                    case 3:
                        SearchFilterByQuestions(keywords);
                        break;
                    case 4:
                        SearchGetTags(keywords);
                        break;
                    case 5:
                        SearchQuestionsByTag(keywords, tag);
                        break;
                    default:
                        Console.WriteLine("Invalid option");
                        break;
                }               
            }
        }

        /// <summary>
        /// This method performs the most basic search, returning all documents that match the text in any field
        /// </summary>
        /// <param name="keywords">Keywords used for searching</param>
        private void SearchBasic(string keywords)
        {
            Console.WriteLine("You searched for: " + keywords);

            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Post>>();

            // Run a basic search, with only keywords
            var posts = solr.Query(new SolrQuery(keywords));

            if (posts.Count == 0)
            {
                Console.WriteLine("No results returned");
                return;
            }

            
            for (int i = 0; i < 10; i++)
            {
                if (i >= posts.Count)
                    break;

                Console.WriteLine(i.ToString() + ": " + posts[i].Title);
                Console.WriteLine("    [Id: " + posts[i].Id + " - Post Type: " + posts[i].PostTypeId + " - Tags: <" + string.Join(",", posts[i].Tags) + "> - Owner: " + posts[i].OwnerUserId +  "]");
                Console.Write(Environment.NewLine);
            }

            // Note: Please observe how many posts are returned without a title. 
            // The reason is that posts contains both questions and answers, and only questions have a title. 
            // If we are interested only by questions, we need to filter by type of post 
            // Please review SearchFilterByQuestions for code sample

            Console.WriteLine(Environment.NewLine);
        }

        /// <summary>
        /// This method performs a basic keyword search, returning all questions that match the text in any field
        /// Additionally, a few new paramters are added for returning only 10 results
        /// </summary>
        /// <param name="keywords">Keywords used for searching the questions</param>
        private void SearchFilterByQuestions(string keywords)
        {
            Console.WriteLine("You searched for questions with: " + keywords);

            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Post>>();

            QueryOptions query_options = new QueryOptions
            {
                Rows = 10,
                StartOrCursor = new StartOrCursor.Start(0),
                FilterQueries = new ISolrQuery[] {
                    new SolrQueryByField("postTypeId", "1"),
                }
            };
            // Construct the query
            SolrQuery query = new SolrQuery(keywords);

            // Run a basic keyword search, filtering for questions only
            var posts = solr.Query(query, query_options);

            if (posts.Count == 0)
            {
                Console.WriteLine("No results returned");
                return;
            }

            int i = 0;
            foreach (Post post in posts)
            {
                Console.WriteLine(i.ToString() + ": " + post.Title);
                Console.WriteLine("[Id: " + posts[i].Id + " - Post Type: " + posts[i].PostTypeId + " - Tags: <" + string.Join(", ", posts[i].Tags) + "> - Owner: " + posts[i].OwnerUserId + "]");
                Console.Write(Environment.NewLine);
                i++;
            }

            // Note: Now we are getting only questions. Look at the tags... wouldn't it be nice to drill down by a particular tag? 
            // And how popular is each one of the tags? Please refer to SearchGetTags for code sample

            Console.WriteLine(Environment.NewLine);
        }

        /// <summary>
        /// This method performs a basic keyword search, returning all questions that match the text in any field
        /// Narrows down by a particular tag
        /// </summary>
        /// <param name="keywords">>Keywords used for searching the questions</param>
        private void SearchGetTags(string keywords)
        {
            Console.WriteLine("You searched for questions with: " + keywords);

            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Post>>();

            QueryOptions query_options = new QueryOptions
            {
                Rows = 10,
                StartOrCursor = new StartOrCursor.Start(0),
                FilterQueries = new ISolrQuery[] {
                    new SolrQueryByField("postTypeId", "1")
                },
                Facet = new FacetParameters // This is where we request which refiners we want
                {
                    Queries = new[] {
                        new SolrFacetFieldQuery("tags")
                    }
                }
            };

            // Construct the query
            SolrQuery query = new SolrQuery(keywords);

            // Run a basic keyword search, filtering for questions only
            var posts = solr.Query(query, query_options);

            if (posts.Count == 0)
            {
                Console.WriteLine("No results returned");
                return;
            }

            Console.WriteLine("Tags Facet:");
            int i = 0;
            foreach (KeyValuePair<string, int> one_tag in posts.FacetFields["tags"])
            {
                Console.WriteLine("  " + one_tag.Key + " (" + one_tag.Value.ToString() + ")");
                i++;
            }

            Console.WriteLine(Environment.NewLine + "Number of tags: " + i.ToString());

            // We are currently displaying all tags. We can control how many are returned, or what is the minimum count.
            // But how do I search by one particular tag?
            // Please refer to SearchQuestionsByTag for code sample

            Console.WriteLine(Environment.NewLine);
        }

        /// <summary>
        /// This method performs a basic keyword search, returning all questions that match the text in any field
        /// Narrows down by a particular tag
        /// </summary>
        /// <param name="keywords">>Keywords used for searching the questions</param>
        /// <param name="tag">Tag that will be used as part of the refiners</param>
        private void SearchQuestionsByTag(string keywords, string tag)
        {
            Console.WriteLine("You searched for questions with: " + keywords);

            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Post>>();

            QueryOptions query_options = new QueryOptions
            {
                Rows = 10,
                StartOrCursor = new StartOrCursor.Start(0),
                FilterQueries = new ISolrQuery[] {
                    new SolrQueryByField("postTypeId", "1"),
                    new SolrQueryByField("tags", tag)
                },
                Facet = new FacetParameters // This is where we filter by tag
                {
                    Queries = new[] { new SolrFacetFieldQuery("tags") }
                }
            };
            // Construct the query
            SolrQuery query = new SolrQuery(keywords);

            // Run a basic keyword search, filtering for questions only
            var posts = solr.Query(query, query_options);

            if (posts.Count == 0)
            {
                Console.WriteLine("No results returned");
                return;
            }

            int i = 0;
            foreach (Post post in posts)
            {
                Console.WriteLine(i.ToString() + ": " + post.Title);
                Console.WriteLine("    [Id: " + posts[i].Id + " - Post Type: " + posts[i].PostTypeId + " - Tags: <" + string.Join(", ", posts[i].Tags) + "> - Owner: " + posts[i].OwnerUserId + "]");
                Console.Write(Environment.NewLine);
                i++;
            }

            // Now show the tags facet
            Console.Write(Environment.NewLine);
            Console.WriteLine("Tags Facet:");
            int j = 0;
            foreach (KeyValuePair<string, int> one_tag in posts.FacetFields["tags"])
            {
                Console.WriteLine("  " + one_tag.Key + " (" + one_tag.Value.ToString() + ")");
                j++;
            }

            Console.WriteLine(Environment.NewLine + "Number of tags: " + j.ToString());

            // Note: Now we are getting only questions. Look at the tags... wouldn't it be nice to drill down by a particular tag? 
            // Please refer to SearchQuestionsByTag for code sample

            Console.WriteLine(Environment.NewLine);
        }
    }
}
