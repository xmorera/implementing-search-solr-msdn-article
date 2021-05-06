using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;
using MSDNArticleDemo.Models;
using SolrNet;

namespace MSDNArticleDemo.Helpers
{
    class HelperFunctions
    {
        public void InitializeSolrNet()
        {
            //Initialize the library in order to operate against the Solr instance.
            Startup.Init<Post>("http://127.0.0.1:8983/solr/msdnarticledemo");
        }

        public void ClearIndex()
        {
            Console.WriteLine("Deleting all documents in the index");

            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Post>>();

            solr.Delete(new SolrQuery("*:*"));
            solr.Commit();

        }
    }
}
