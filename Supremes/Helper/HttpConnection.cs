/*
 * This code is derived from jsoup 1.8.1 (http://jsoup.org/news/release-1.8.1)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NSoup;
using NSoup.Helper;
using NSoup.Nodes;
using NSoup.Parser;
using Sharpen;

namespace NSoup.Helper
{
	/// <summary>
	/// Implementation of
	/// <see cref="NSoup.Connection">NSoup.Connection</see>
	/// .
	/// </summary>
	/// <seealso cref="NSoup.Jsoup.Connect(string)">NSoup.Jsoup.Connect(string)</seealso>
	public class HttpConnection : Connection
	{
		private const int HTTP_TEMP_REDIR = 307;

		// http/1.1 temporary redirect, not in Java's set.
		public static Connection Connect(string url)
		{
			Connection con = new NSoup.Helper.HttpConnection();
			con.Url(url);
			return con;
		}

		public static Connection Connect(Uri url)
		{
			Connection con = new NSoup.Helper.HttpConnection();
			con.Url(url);
			return con;
		}

		private static string EncodeUrl(string url)
		{
			if (url == null)
			{
				return null;
			}
			return url.ReplaceAll(" ", "%20");
		}

		private NSoup.Request req;

		private NSoup.Response res;

		public HttpConnection()
		{
			req = new HttpConnection.Request();
			res = new HttpConnection.Response();
		}

		public Connection Url(Uri url)
		{
			req.Url(url);
			return this;
		}

		public Connection Url(string url)
		{
			Validate.NotEmpty(url, "Must supply a valid URL");
			try
			{
				req.Url(new Uri(EncodeUrl(url)));
			}
			catch (UriFormatException e)
			{
				throw new ArgumentException("Malformed URL: " + url, e);
			}
			return this;
		}

		public Connection UserAgent(string userAgent)
		{
			Validate.NotNull(userAgent, "User agent must not be null");
			req.Header("User-Agent", userAgent);
			return this;
		}

		public Connection Timeout(int millis)
		{
			req.Timeout(millis);
			return this;
		}

		public Connection MaxBodySize(int bytes)
		{
			req.MaxBodySize(bytes);
			return this;
		}

		public Connection FollowRedirects(bool followRedirects)
		{
			req.FollowRedirects(followRedirects);
			return this;
		}

		public Connection Referrer(string referrer)
		{
			Validate.NotNull(referrer, "Referrer must not be null");
			req.Header("Referer", referrer);
			return this;
		}

		public Connection _Method(ConnectionMethod method)
		{
			req.Method(method);
			return this;
		}

		public Connection IgnoreHttpErrors(bool ignoreHttpErrors)
		{
			req.IgnoreHttpErrors(ignoreHttpErrors);
			return this;
		}

		public Connection IgnoreContentType(bool ignoreContentType)
		{
			req.IgnoreContentType(ignoreContentType);
			return this;
		}

		public Connection Data(string key, string value)
		{
			req.Data(HttpConnection.KeyVal.Create(key, value));
			return this;
		}

		public Connection Data(IDictionary<string, string> data)
		{
			Validate.NotNull(data, "Data map must not be null");
			foreach (KeyValuePair<string, string> entry in data.EntrySet())
			{
				req.Data(HttpConnection.KeyVal.Create(entry.Key, entry.Value));
			}
			return this;
		}

		public Connection Data(params string[] keyvals)
		{
			Validate.NotNull(keyvals, "Data key value pairs must not be null");
			Validate.IsTrue(keyvals.Length % 2 == 0, "Must supply an even number of key value pairs"
				);
			for (int i = 0; i < keyvals.Length; i += 2)
			{
				string key = keyvals[i];
				string value = keyvals[i + 1];
				Validate.NotEmpty(key, "Data key must not be empty");
				Validate.NotNull(value, "Data value must not be null");
				req.Data(HttpConnection.KeyVal.Create(key, value));
			}
			return this;
		}

		public Connection Data(ICollection<NSoup.KeyVal> data)
		{
			Validate.NotNull(data, "Data collection must not be null");
			foreach (KeyVal entry in data)
			{
				req.Data(entry);
			}
			return this;
		}

		public Connection Header(string name, string value)
		{
			req.Header(name, value);
			return this;
		}

		public Connection Cookie(string name, string value)
		{
			req.Cookie(name, value);
			return this;
		}

		public Connection Cookies(IDictionary<string, string> cookies)
		{
			Validate.NotNull(cookies, "Cookie map must not be null");
			foreach (KeyValuePair<string, string> entry in cookies.EntrySet())
			{
				req.Cookie(entry.Key, entry.Value);
			}
			return this;
		}

		public Connection Parser(NSoup.Parser.Parser parser)
		{
			req.Parser(parser);
			return this;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public Document Get()
		{
			req.Method(ConnectionMethod.GET);
			Execute();
			return res.Parse();
		}

		/// <exception cref="System.IO.IOException"></exception>
		public Document Post()
		{
			req.Method(ConnectionMethod.POST);
			Execute();
			return res.Parse();
		}

		/// <exception cref="System.IO.IOException"></exception>
		public NSoup.Response Execute()
		{
			res = HttpConnection.Response.Execute(req);
			return res;
		}

		public NSoup.Request _Request()
		{
			return req;
		}

		public Connection _Request(NSoup.Request request)
		{
			req = request;
			return this;
		}

		public NSoup.Response _Response()
		{
			return res;
		}

		public Connection _Response(NSoup.Response response)
		{
			res = response;
			return this;
		}

        public abstract class Base<T> : NSoup.Base<T> where T : Base<T>, NSoup.Base<T>
        {
			internal Uri url;

			internal ConnectionMethod method;

			internal IDictionary<string, string> headers;

			internal IDictionary<string, string> cookies;

			public Base()
			{
				headers = new LinkedHashMap<string, string>();
				cookies = new LinkedHashMap<string, string>();
			}

			public Uri Url()
			{
				return url;
			}

			public T Url(Uri url)
			{
				Validate.NotNull(url, "URL must not be null");
				this.url = url;
				return (T)(object)this;
			}

			public ConnectionMethod Method()
			{
				return method;
			}

			public T Method(ConnectionMethod method)
			{
				Validate.NotNull(method, "Method must not be null");
				this.method = method;
				return (T)(object)this;
			}

			public string Header(string name)
			{
				Validate.NotNull(name, "Header name must not be null");
				return GetHeaderCaseInsensitive(name);
			}

			public T Header(string name, string value)
			{
				Validate.NotEmpty(name, "Header name must not be empty");
				Validate.NotNull(value, "Header value must not be null");
				RemoveHeader(name);
				// ensures we don't get an "accept-encoding" and a "Accept-Encoding"
				headers.Put(name, value);
				return (T)(object)this;
			}

			public bool HasHeader(string name)
			{
				Validate.NotEmpty(name, "Header name must not be empty");
				return GetHeaderCaseInsensitive(name) != null;
			}

			public T RemoveHeader(string name)
			{
				Validate.NotEmpty(name, "Header name must not be empty");
				KeyValuePair<string, string> entry = ScanHeaders(name);
				// remove is case insensitive too
				if (entry.Key != null)
				{
					Sharpen.Collections.Remove(headers, entry.Key);
				}
				// ensures correct case
				return (T)(object)this;
			}

			public IDictionary<string, string> Headers()
			{
				return headers;
			}

			private string GetHeaderCaseInsensitive(string name)
			{
				Validate.NotNull(name, "Header name must not be null");
				// quick evals for common case of title case, lower case, then scan for mixed
				string value = headers.Get(name);
				if (value == null)
				{
					value = headers.Get(name.ToLower());
				}
				if (value == null)
				{
					KeyValuePair<string, string> entry = ScanHeaders(name);
					if (entry.Key != null)
					{
						value = entry.Value;
					}
				}
				return value;
			}

			private KeyValuePair<string, string> ScanHeaders(string name)
			{
				string lc = name.ToLower();
				foreach (KeyValuePair<string, string> entry in headers.EntrySet())
				{
					if (entry.Key.ToLower().Equals(lc))
					{
						return entry;
					}
				}
				return default(KeyValuePair<string, string>);
			}

			public string Cookie(string name)
			{
				Validate.NotNull(name, "Cookie name must not be null");
				return cookies.Get(name);
			}

			public T Cookie(string name, string value)
			{
				Validate.NotEmpty(name, "Cookie name must not be empty");
				Validate.NotNull(value, "Cookie value must not be null");
				cookies.Put(name, value);
				return (T)(object)this;
			}

			public bool HasCookie(string name)
			{
				Validate.NotEmpty("Cookie name must not be empty");
				return cookies.ContainsKey(name);
			}

			public T RemoveCookie(string name)
			{
				Validate.NotEmpty("Cookie name must not be empty");
				Sharpen.Collections.Remove(cookies, name);
				return (T)(object)this;
			}

			public IDictionary<string, string> Cookies()
			{
				return cookies;
			}
		}

		public class Request : HttpConnection.Base<Request>, NSoup.Request
		{
			private int timeoutMilliseconds;

			private int maxBodySizeBytes;

			private bool followRedirects;

			private ICollection<NSoup.KeyVal> data;

			private bool ignoreHttpErrors = false;

			private bool ignoreContentType = false;

			private NSoup.Parser.Parser parser;

			public Request()
			{
				timeoutMilliseconds = 3000;
				maxBodySizeBytes = 1024 * 1024;
				// 1MB
				followRedirects = true;
				data = new AList<NSoup.KeyVal>();
				method = ConnectionMethod.GET;
				headers.Put("Accept-Encoding", "gzip");
				parser = NSoup.Parser.Parser.HtmlParser();
			}

			public int Timeout()
			{
				return timeoutMilliseconds;
			}

			public NSoup.Request Timeout(int millis)
			{
				Validate.IsTrue(millis >= 0, "Timeout milliseconds must be 0 (infinite) or greater");
				timeoutMilliseconds = millis;
				return this;
			}

			public int MaxBodySize()
			{
				return maxBodySizeBytes;
			}

			public NSoup.Request MaxBodySize(int bytes)
			{
				Validate.IsTrue(bytes >= 0, "maxSize must be 0 (unlimited) or larger");
				maxBodySizeBytes = bytes;
				return this;
			}

			public bool FollowRedirects()
			{
				return followRedirects;
			}

			public NSoup.Request FollowRedirects(bool followRedirects)
			{
				this.followRedirects = followRedirects;
				return this;
			}

			public bool IgnoreHttpErrors()
			{
				return ignoreHttpErrors;
			}

			public NSoup.Request IgnoreHttpErrors(bool ignoreHttpErrors)
			{
				this.ignoreHttpErrors = ignoreHttpErrors;
				return this;
			}

			public bool IgnoreContentType()
			{
				return ignoreContentType;
			}

			public NSoup.Request IgnoreContentType(bool ignoreContentType)
			{
				this.ignoreContentType = ignoreContentType;
				return this;
			}

			public NSoup.Request Data(NSoup.KeyVal keyval)
			{
				Validate.NotNull(keyval, "Key val must not be null");
				data.AddItem(keyval);
				return this;
			}

			public ICollection<NSoup.KeyVal> Data()
			{
				return data;
			}

			public NSoup.Request Parser(NSoup.Parser.Parser parser)
			{
				this.parser = parser;
				return this;
			}

			public NSoup.Parser.Parser Parser()
			{
				return parser;
			}

            NSoup.Request NSoup.Base<NSoup.Request>.Url(Uri url)
            {
                throw new NotImplementedException();
            }

            NSoup.Request NSoup.Base<NSoup.Request>.Method(ConnectionMethod method)
            {
                throw new NotImplementedException();
            }

            NSoup.Request NSoup.Base<NSoup.Request>.Header(string name, string value)
            {
                throw new NotImplementedException();
            }

            NSoup.Request NSoup.Base<NSoup.Request>.RemoveHeader(string name)
            {
                throw new NotImplementedException();
            }

            NSoup.Request NSoup.Base<NSoup.Request>.Cookie(string name, string value)
            {
                throw new NotImplementedException();
            }

            NSoup.Request NSoup.Base<NSoup.Request>.RemoveCookie(string name)
            {
                throw new NotImplementedException();
            }
        }

		public class Response : Base<Response>, NSoup.Response
		{
			private const int MAX_REDIRECTS = 20;

			private int statusCode;

			private string statusMessage;

			private ByteBuffer byteData;

			private string charset;

			private string contentType;

			private bool executed = false;

			private int numRedirects = 0;

			private Request req;

			private static readonly Sharpen.Pattern xmlContentTypeRxp = Sharpen.Pattern.Compile
				("application/\\w+\\+xml.*");

			public Response() : base()
			{
			}

			/// <exception cref="System.IO.IOException"></exception>
			private Response(HttpConnection.Response previousResponse) : base()
			{
				if (previousResponse != null)
				{
					numRedirects = previousResponse.numRedirects + 1;
					if (numRedirects >= MAX_REDIRECTS)
					{
						throw new IOException(string.Format("Too many redirects occurred trying to load URL %s"
							, previousResponse.Url()));
					}
				}
			}

			/// <exception cref="System.IO.IOException"></exception>
			internal static Response Execute(NSoup.Request req)
			{
				return Execute(req, null);
			}

			/// <exception cref="System.IO.IOException"></exception>
			internal static Response Execute(NSoup.Request req, Response previousResponse)
			{
				Validate.NotNull(req, "Request must not be null");
				string protocol = req.Url().Scheme;
				if (!protocol.Equals("http") && !protocol.Equals("https"))
				{
					throw new UriFormatException("Only http & https protocols supported");
				}
				// set up the request for execution
				if (req.Method() == ConnectionMethod.GET && req.Data().Count > 0)
				{
					SerialiseRequestUrl(req);
				}
				// appends query string
				HttpURLConnection conn = CreateConnection(req);
				HttpConnection.Response res;
				try
				{
					conn.Connect();
					if (req.Method() == ConnectionMethod.POST)
					{
						WritePost(req.Data(), conn.GetOutputStream());
					}
					int status = conn.GetResponseCode();
					bool needsRedirect = false;
					if (status != HttpURLConnection.HTTP_OK)
					{
						if (status == HttpURLConnection.HTTP_MOVED_TEMP || status == HttpURLConnection.HTTP_MOVED_PERM
							 || status == HttpURLConnection.HTTP_SEE_OTHER || status == HTTP_TEMP_REDIR)
						{
							needsRedirect = true;
						}
						else
						{
							if (!req.IgnoreHttpErrors())
							{
								throw new HttpStatusException("HTTP error fetching URL", status, req.Url().ToString
									());
							}
						}
					}
					res = new HttpConnection.Response(previousResponse);
					res.SetupFromConnection(conn, previousResponse);
					if (needsRedirect && req.FollowRedirects())
					{
						req.Method(ConnectionMethod.GET);
						// always redirect with a get. any data param from original req are dropped.
						req.Data().Clear();
						string location = res.Header("Location");
						if (location != null && location.StartsWith("http:/") && location[6] != '/')
						{
							// fix broken Location: http:/temp/AAG_New/en/index.php
							location = Sharpen.Runtime.Substring(location, 6);
						}
						req.Url(new Uri(req.Url(), EncodeUrl(location)));
						foreach (KeyValuePair<string, string> cookie in res.cookies.EntrySet())
						{
							// add response cookies to request (for e.g. login posts)
							req.Cookie(cookie.Key, cookie.Value);
						}
						return Execute(req, res);
					}
					res.req = req;
					// check that we can handle the returned content type; if not, abort before fetching it
					string contentType = res.ContentType();
					if (contentType != null && !req.IgnoreContentType() && !contentType.StartsWith("text/"
						) && !contentType.StartsWith("application/xml") && !xmlContentTypeRxp.Matcher(contentType
						).Matches())
					{
						throw new UnsupportedMimeTypeException("Unhandled content type. Must be text/*, application/xml, or application/xhtml+xml"
							, contentType, req.Url().ToString());
					}
					InputStream bodyStream = null;
					InputStream dataStream = null;
					try
					{
						dataStream = conn.GetErrorStream() != null ? conn.GetErrorStream() : conn.GetInputStream
							();
						bodyStream = res.HasHeader("Content-Encoding") && Sharpen.Runtime.EqualsIgnoreCase
							(res.Header("Content-Encoding"), "gzip") ? new BufferedInputStream(new GZIPInputStream
							(dataStream)) : new BufferedInputStream(dataStream);
						res.byteData = DataUtil.ReadToByteBuffer(bodyStream, req.MaxBodySize());
						res.charset = DataUtil.GetCharsetFromContentType(res.contentType);
					}
					finally
					{
						// may be null, readInputStream deals with it
						if (bodyStream != null)
						{
							bodyStream.Close();
						}
						if (dataStream != null)
						{
							dataStream.Close();
						}
					}
				}
				finally
				{
					// per Java's documentation, this is not necessary, and precludes keepalives. However in practise,
					// connection errors will not be released quickly enough and can cause a too many open files error.
					conn.Disconnect();
				}
				res.executed = true;
				return res;
			}

			public int StatusCode()
			{
				return statusCode;
			}

			public string StatusMessage()
			{
				return statusMessage;
			}

			public string Charset()
			{
				return charset;
			}

			public string ContentType()
			{
				return contentType;
			}

			/// <exception cref="System.IO.IOException"></exception>
			public Document Parse()
			{
				Validate.IsTrue(executed, "Request must be executed (with .execute(), .get(), or .post() before parsing response"
					);
				Document doc = DataUtil.ParseByteData(byteData, charset, url.ToString(), req
					.Parser());
				byteData.Rewind();
				charset = doc.OutputSettings().Charset().Name();
				// update charset from meta-equiv, possibly
				return doc;
			}

			public string Body()
			{
				Validate.IsTrue(executed, "Request must be executed (with .execute(), .get(), or .post() before getting response body"
					);
				// charset gets set from header on execute, and from meta-equiv on parse. parse may not have happened yet
				string body;
				if (charset == null)
				{
					body = Sharpen.Extensions.GetEncoding(DataUtil.defaultCharset).Decode(byteData).ToString
						();
				}
				else
				{
					body = Sharpen.Extensions.GetEncoding(charset).Decode(byteData).ToString();
				}
				byteData.Rewind();
				return body;
			}

			public byte[] BodyAsBytes()
			{
				Validate.IsTrue(executed, "Request must be executed (with .execute(), .get(), or .post() before getting response body"
					);
				return ((byte[])byteData.Array());
			}

			// set up connection defaults, and details from request
			/// <exception cref="System.IO.IOException"></exception>
			private static HttpURLConnection CreateConnection(NSoup.Request req)
			{
				HttpURLConnection conn = (HttpURLConnection)req.Url().OpenConnection();
				conn.SetRequestMethod(req.Method().ToString());
				conn.SetInstanceFollowRedirects(false);
				// don't rely on native redirection support
				conn.SetConnectTimeout(req.Timeout());
				conn.SetReadTimeout(req.Timeout());
				if (req.Method() == ConnectionMethod.POST)
				{
					conn.SetDoOutput(true);
				}
				if (req.Cookies().Count > 0)
				{
					conn.AddRequestProperty("Cookie", GetRequestCookieString(req));
				}
				foreach (KeyValuePair<string, string> header in req.Headers().EntrySet())
				{
					conn.AddRequestProperty(header.Key, header.Value);
				}
				return conn;
			}

			// set up url, method, header, cookies
			/// <exception cref="System.IO.IOException"></exception>
			private void SetupFromConnection(HttpURLConnection conn, NSoup.Response previousResponse)
			{
				method = (ConnectionMethod)Enum.Parse(typeof(ConnectionMethod), conn.GetRequestMethod());
				url = conn.GetURL();
				statusCode = conn.GetResponseCode();
				statusMessage = conn.GetResponseMessage();
				contentType = conn.GetContentType();
				IDictionary<string, IList<string>> resHeaders = conn.GetHeaderFields();
				ProcessResponseHeaders(resHeaders);
				// if from a redirect, map previous response cookies into this response
				if (previousResponse != null)
				{
					foreach (KeyValuePair<string, string> prevCookie in previousResponse.Cookies().EntrySet
						())
					{
						if (!HasCookie(prevCookie.Key))
						{
							Cookie(prevCookie.Key, prevCookie.Value);
						}
					}
				}
			}

			internal virtual void ProcessResponseHeaders(IDictionary<string, IList<string>> resHeaders)
			{
				foreach (KeyValuePair<string, IList<string>> entry in resHeaders.EntrySet())
				{
					string name = entry.Key;
					if (name == null)
					{
						continue;
					}
					// http/1.1 line
					IList<string> values = entry.Value;
					if (Sharpen.Runtime.EqualsIgnoreCase(name, "Set-Cookie"))
					{
						foreach (string value in values)
						{
							if (value == null)
							{
								continue;
							}
							TokenQueue cd = new TokenQueue(value);
							string cookieName = cd.ChompTo("=").Trim();
							string cookieVal = cd.ConsumeTo(";").Trim();
							if (cookieVal == null)
							{
								cookieVal = string.Empty;
							}
							// ignores path, date, domain, secure et al. req'd?
							// name not blank, value not null
							if (cookieName != null && cookieName.Length > 0)
							{
								Cookie(cookieName, cookieVal);
							}
						}
					}
					else
					{
						// only take the first instance of each header
						if (!values.IsEmpty())
						{
							Header(name, values[0]);
						}
					}
				}
			}

			/// <exception cref="System.IO.IOException"></exception>
			private static void WritePost(ICollection<KeyVal> data, OutputStream outputStream
				)
			{
				OutputStreamWriter w = new OutputStreamWriter(outputStream, DataUtil.defaultCharset
					);
				bool first = true;
				foreach (KeyVal keyVal in data)
				{
					if (!first)
					{
						w.Append('&');
					}
					else
					{
						first = false;
					}
					w.Write(URLEncoder.Encode(keyVal.Key(), DataUtil.defaultCharset));
					w.Write('=');
					w.Write(URLEncoder.Encode(keyVal.Value(), DataUtil.defaultCharset));
				}
				w.Close();
			}

			private static string GetRequestCookieString(NSoup.Request req)
			{
				StringBuilder sb = new StringBuilder();
				bool first = true;
				foreach (KeyValuePair<string, string> cookie in req.Cookies().EntrySet())
				{
					if (!first)
					{
						sb.Append("; ");
					}
					else
					{
						first = false;
					}
					sb.Append(cookie.Key).Append('=').Append(cookie.Value);
				}
				// todo: spec says only ascii, no escaping / encoding defined. validate on set? or escape somehow here?
				return sb.ToString();
			}

			// for get url reqs, serialise the data map into the url
			/// <exception cref="System.IO.IOException"></exception>
			private static void SerialiseRequestUrl(NSoup.Request req)
			{
				Uri @in = req.Url();
				StringBuilder url = new StringBuilder();
				bool first = true;
				// reconstitute the query, ready for appends
				url.Append(@in.Scheme).Append("://").Append(@in.Authority).Append(@in.AbsolutePath).Append("?");
				// includes host, port
				if (@in.GetQuery() != null)
				{
					url.Append(@in.Query);
					first = false;
				}
				foreach (NSoup.KeyVal keyVal in req.Data())
				{
					if (!first)
					{
						url.Append('&');
					}
					else
					{
						first = false;
					}
					url.Append(URLEncoder.Encode(keyVal.Key(), DataUtil.defaultCharset)).Append('=').
						Append(URLEncoder.Encode(keyVal.Value(), DataUtil.defaultCharset));
				}
				req.Url(new Uri(url.ToString()));
				req.Data().Clear();
			}
			// moved into url as get params
		}

		private class KeyVal : NSoup.KeyVal
		{
			private string key;

			private string value;

			public static KeyVal Create(string key, string value)
			{
				Validate.NotEmpty(key, "Data key must not be empty");
				Validate.NotNull(value, "Data value must not be null");
				return new KeyVal(key, value);
			}

			private KeyVal(string key, string value)
			{
				this.key = key;
				this.value = value;
			}

			public NSoup.KeyVal Key(string key)
			{
				Validate.NotEmpty(key, "Data key must not be empty");
				this.key = key;
				return this;
			}

			public string Key()
			{
				return key;
			}

			public NSoup.KeyVal Value(string value)
			{
				Validate.NotNull(value, "Data value must not be null");
				this.value = value;
				return this;
			}

			public string Value()
			{
				return value;
			}

			public override string ToString()
			{
				return key + "=" + value;
			}
		}
	}
}
