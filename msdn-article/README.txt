This is the code download for the MSDN Article 'Enterprise Search - Why You Need It and How to Get It'. Before running the code, you need to have Solr and download the sample data. Please refer to 'MUST-ADD-Posts.xml.txt' for instructions on the data.

Demo Steps
-- These are prerequisite steps... just what's required to get what you need for your environment --
#1 Download Solr
Obtain it from http://lucene.apache.org/solr/downloads.html


#2 Start Solr
Unzip in a folder. Confirm that JAVA 8 is installed, and that JAVA_HOME is set appropriately. Go to the root of the extracted folder and run:
	> bin/solr.cmd start

Navigate to the Solr Admin UI located at http://localhost:8983

When you want to shut down Solr, you can run
	> bin/solr.cmd stop


#3 Get SolrNet
Easiest way is to install from NuGet. The package that you need is SolrNet. There are other packages that you can download which are for using other inversion control mechanisms and other functionality, like SolrCloud.


#4 Get the Source Data
The data that we will index and make searchable is the posts from StackOverflow... or either one of the StackExchange sites. Why? Because everybody knows StackOverflow, has used it, and has searched for an answer and found it within StackOverflow. 

The data dumps are provided for free, and available in https://archive.org/details/stackexchange. The posts from each site follow the same format, so you can load a few hundred records from one of the minor sites or many million from the main StackOverflow site.  The format of the XML file is the same, regardless of the site that you pick.

We will use the datascience.stackexchange.com data.


#5 Prepare the Index
Solr, by default, uses a Managed Schema. This means that it can create fields for your data as it is ingested. Alternatively, you can configure your index and create specifically the fields that you need, with the types that best suit your needs... which is similar to creating columns in a database. This second approach is what we are going to use for multiple reasons that are covered in the article.

We need to explicitly configure a ClassicIndexSchemaFactory, which requires the use of a schema.xml configuration file, which needs to be edited manually and it is loaded when the collection is loaded.

The steps are:
- Create a folder called 'msdnarticledemo' in '<solr>\server\solr\', where '<solr>' is the folder where you unzipped Solr.
- Copy '<solr>\server\solr\configsets\_default' folder contents, a folder called 'conf', inside '<solr>\server\solr\msdnarticledemo' 
- Add the following line to solrconfig.xml to use classic schema (you can add before the codecFactory)
	<schemaFactory class="ClassicIndexSchemaFactory"/>
- Comment out the updateRequestProcessorChain with name="add-unknown-fields-to-the-schema" entry
- Commment out the updateProcessor class="solr.AddSchemaFieldsUpdateProcessorFactory" name="add-schema-fields" entry. Remove the comment inside this xml node as well, given that '--' is not allowed within xml comments.
- Add core.properties file to 'msdnarticledemo' folder. The contents of the file should be:
	name=msdnarticledemo
- Rename 'managed-schema' to 'schema.xml', and open for editing.  Add the following fields:
    <field name="id" type="string" indexed="true" stored="true" required="true" multiValued="false" />	

	<field name="postTypeId" type="pint" indexed="true" stored="true" />

	<field name="title" type="text_gen_sort" indexed="true" stored="true" multiValued="false"/>

	<field name="body" type="text_general" indexed="false" stored="true" multiValued="false"/>

	<field name="tags" type="string" indexed="true" stored="true" multiValued="true"/>

	<field name="postScore" type="pfloat" indexed="true" stored="true"/>
	
	<field name="ownerUserId" type="pint" indexed="true" stored="true" />
	
	<field name="answerCount" type="pint" indexed="true" stored="true" />
	
	<field name="commentCount" type="pint" indexed="true" stored="true" />
	
	<field name="favoriteCount" type="pint" indexed="true" stored="true" />
	
	<field name="viewCount" type="pint" indexed="true" stored="true" />	
	
	<field name="creationDate" type="pdate" indexed="true" stored="true" />
	
	<field name="lastActivityDate" type="pdate" indexed="true" stored="true" />
	
	<field name="closedDate" type="pdate" indexed="true" stored="true" />


Also, we need to specify which fields we are going to search. As a start, we will indicate that all fields will be searched by using a copyfield. In a production application this is not the recommended practice, but it will get us started. 
	<copyField source="*" dest="_text_"/>

Restart Solr, to load the core. This is quite easy to do by issuing the following command from the command line in the Solr folder:
	> bin\solr.cmd restart -p 8983


-- Here is where we start building our application --
#6 Create Application and Initialize Solr
At this point we create a console application. It contains two basic functionalities: index and search. This simple console application will allow to clear the documents in the index, add documents to the index, and start a basic search function.

We also need to initialize the library to work with Solr. This should also be called once during the lifetime of your application.
	Startup.Init<Post>("http://127.0.0.1:8983/solr/msdnarticledemo");


#7 Create the Models
It is required to create a model that represents your documents. This is done by creating a POCO object with the fields, and an attribute. Here is a complete model that represents each post. As you can see, it matches what you declared in the schema.xml.
    class Post
    {
        [SolrUniqueKey("id")]
        public string Id { get; set; }

        [SolrField("postTypeId")]
        public int PostTypeId { get; set; }

        [SolrField("title")]
        public string Title { get; set; }

        [SolrField("body")]
        public string Body { get; set; }

        [SolrField("tags")]
        public ICollection<string> Tags { get; set; } = new List<string>();

		[SolrField("postScore")]
        public float PostScore { get; set; }

        [SolrField("ownerUserId")]
        public int? OwnerUserId { get; set; }

        [SolrField("answerCount")]
        public int? AnswerCount { get; set; }

        [SolrField("commentCount")]
        public int CommentCount { get; set; }

        [SolrField("favoriteCount")]
        public int? FavoriteCount { get; set; }

        [SolrField("viewCount")]
        public int? ViewCount { get; set; }

        [SolrField("creationDate")]
        public DateTime CreationDate { get; set; }

        [SolrField("lastActivityDate")]
        public DateTime LastActivityDate { get; set; }

        [SolrField("closedDate")]
        public DateTime? ClosedDate { get; set; }
	}

As a note, a document is just how in the search engine world we refer to a record.


#8 Index Documents
To index the documents we need to follow a few steps. First we need to obtain the SolrNet service instance which allows us to issue any supported operations.
	var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Post>>();

Then we need to read the Posts.xml into an XMLDocument. Then, we need to iterate over each node, creating a new Post object, extract each attribute from the XMLNode,  assigning it to the new Post object. 

This is demonstrated with Id and Title below. Please note that we are checking for null values.	   
	post.Id = node.Attributes["Id"].Value;
	if (node.Attributes["Title"] != null)
	{
		post.Title = node.Attributes["Title"].Value;
	}

It is possible to add each Post object to Solr using:
	solr.Add(post);

Alternatively, we can create a posts collection, and add them all at once using this function:
	solr.AddRange(post);

Regardless of which method you use, it has been observed in many production deployments that adding documents in batches of 100 tends to help with performance. This is however just an observation from a series of projects and through performance testing, however your mileage may vary.

Another important detail is that adding a document does not make it searchable. It is necessary to commit the documents:
    solr.Commit();


# 9 Search Documents by Keyword
The most basic search is simply searching by text. Given that we have already indexed a good amount of documents, and we used a copyfield to search a default field, every search that we make will match any documents that contain the word or words that are sent to the search engine. This is the statement that will execute the search, and it will return a collection of all results:
	var posts = solr.Query(new SolrQuery(keywords));

Then, we iterate over posts and extract or write the information that we need. I will write out the position, the title, id, tags, date, and owner id.

As an example, run a search with 'scikit'. Did you notice something? Yes... not all documents have a title. The reason is that the posts data contain both questions and answers... and only questions have titles. Let's then modify our search so that it brings back only questions.


#10 Search Questions by Keyword
A common scenario that you might run into is that you may want to have your users search only against a subset of the data.  For example, in this case we are looking for particular questions that might be similar to the ones that we have. In a company, you may want your users to search based on which department they belong to or physical location. 

In cases like this one, we can narrow down by a particular field in the document's metadata, in this case post type id.

The way to narrow down by a particular metadata field is to use a filter query. For this, you need to create an object that contains the query by field, specifying postTypeId equals to 1 - which is the value of a question.
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

Here we can notice the tags for each document. However, it would be nice to get a big picture of which are the most popular tags. Let me show you how.


#11 Get Tags
A very useful way to help users narrow down results is by presenting them with facets - also called refiners. They are very common, especially in ecommerce. For example, in a site that sells computers, some common facets could be brand, amount of RAM, type of processor, and more.

In our case, we may want to narrow down the search by posts related to a specific tag. How do I turn on faceting? Simple create a new FacetParameters, that contains a new SolrFacetFieldQuery, where you specify which is the field that you want to use as facet. As mentioned, we will use tags.
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
			
Now that we know which are the tags that are available and how may times each one occurs... how do we drill down by a particular facet?

#12 Search Questions By Tag
Now that we know how to search by keyword, on questions, and have the tag refiner, we may want to search by keyword only on those questions that have a particular tag. This is quite simple. We only need to add a new filter query, specifying only tags of a particular tag.  

    QueryOptions query_options = new QueryOptions
    {
        Rows = 10,
        StartOrCursor = new StartOrCursor.Start(0),
        FilterQueries = new ISolrQuery[] {
            new SolrQueryByField("postTypeId", "1"),
			new SolrQueryByField("tags", tag)
        },
        Facet = new FacetParameters // This is where we request which refiners we want
        {
            Queries = new[] {
                new SolrFacetFieldQuery("tags")
            }
        }
    };
	
	
And with that we conclude this simple example on how to create a very basic search with Solr and SolrNet.

by Xavier Morera / www.xaviermorera.com / @xmorera