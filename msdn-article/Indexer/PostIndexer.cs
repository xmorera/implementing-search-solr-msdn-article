using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CommonServiceLocator;
using MSDNArticleDemo.Models;
using SolrNet;

namespace MSDNArticleDemo.Indexer
{
    class PostIndexer
    {
        public void IndexDocuments()
        {
            Console.WriteLine("Indexing initialized");

            //Ask the service locator for the SolrNet service instance which allows you to issue any supported operation:
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Post>>();

            //Read Posts xml
            XmlNodeList nodes = GetNodes();

            List<Post> posts = new List<Post>();
            int index = 1;

            //Measure indexing time
            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (XmlNode node in nodes)
            {
                var post = CreatePost(node);

                posts.Add(post);

                //Add documents in batches of 100 to improve performance
                if (index % 100 == 0)
                {
                    solr.AddRange(posts);
                    Console.WriteLine("Indexing documents [{0} to {1}] out of {2}", index - 100, index, nodes.Count);
                    solr.Commit();
                    posts.Clear();
                }
                index++;
            }

            //In case any document is pending
            if (posts.Any())
            {
                solr.AddRange(posts);
                Console.WriteLine("Indexing documents up to {0} out of {1}", index, nodes.Count);
                solr.Commit();
            }

            //End measuring time
            sw.Stop();
            Console.WriteLine("Elapsed={0}", sw.Elapsed);
        }

        private Post CreatePost(XmlNode node)
        {
            Post post = new Post();

            post.Id = node.Attributes["Id"].Value;
            post.PostTypeId = Int32.Parse(node.Attributes["PostTypeId"].Value);
            post.Body = node.Attributes["Body"].Value;
            post.PostScore = float.Parse(node.Attributes["Score"].Value);
            post.CommentCount = Int32.Parse(node.Attributes["CommentCount"].Value);
            post.CreationDate = DateTime.Parse(node.Attributes["CreationDate"].Value);
            post.LastActivityDate = DateTime.Parse(node.Attributes["LastActivityDate"].Value);

            //Some fields might not contain values
            if (node.Attributes["Title"] != null)
            {
                post.Title = node.Attributes["Title"].Value;
            }

            if (node.Attributes["OwnerUserId"] != null)
            {
                post.OwnerUserId = Int32.Parse(node.Attributes["OwnerUserId"].Value);
            }

            if (node.Attributes["AnswerCount"] != null)
            {
                post.AnswerCount = Int32.Parse(node.Attributes["AnswerCount"].Value);
            }

            if (node.Attributes["FavoriteCount"] != null)
            {
                post.FavoriteCount = Int32.Parse(node.Attributes["FavoriteCount"].Value);
            }

            if (node.Attributes["ViewCount"] != null)
            {
                post.ViewCount = Int32.Parse(node.Attributes["ViewCount"].Value);
            }

            if (node.Attributes["ClosedDate"] != null)
            {
                post.ClosedDate = DateTime.Parse(node.Attributes["ClosedDate"].Value);
            }

            if (node.Attributes["Tags"] != null)
            {
                post.Tags = node.Attributes["Tags"].Value.Split(new char[] { '<', '>' })
                   .Where(t => !string.IsNullOrEmpty(t)).ToList();
            }

            return post;
        }

        private XmlNodeList GetNodes()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("Posts.xml");
            return xml.DocumentElement.SelectNodes("/posts/*");
        }
    }
}
