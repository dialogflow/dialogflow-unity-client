using System;
using System.Collections;
using System.Net;
using System.IO;
using System.Text;

namespace ApiAiSDK
{
	public class MultipartHttpClient
	{

		private const string delimiter = "--";
		private string boundary = "SwA" + DateTime.UtcNow.Ticks + "SwA";
		private HttpWebRequest request;
		private BinaryWriter os;

		public MultipartHttpClient (HttpWebRequest request)
		{
			this.request = request;
		}

		public void connect()
		{
			request.ContentType = "multipart/form-data; boundary=" + boundary;
			request.SendChunked = true;
			request.KeepAlive = true;

			os = new BinaryWriter (request.GetRequestStream (), Encoding.UTF8);
		}

		public void addStringPart(string paramName, string data)
		{
			os.Write (delimiter + boundary + "\r\n");
			os.Write ("Content-Type: application/json\r\n");
			os.Write ("Content-Disposition: form-data; name=\"" + paramName + "\"\r\n");
			os.Write ("\r\n" + data + "\r\n");
		}

		public void addFilePart(string paramName, string fileName, Stream data)
		{
			os.Write (delimiter + boundary + "\r\n");
			os.Write ("Content-Disposition: form-data; name=\"" + paramName + "\"; filename=\"" + fileName + "\"\r\n");
			os.Write ("Content-Type: audio/wav\r\n");

			os.Write ("\r\n");

			int bufferSize = 4096;
			byte[] buffer = new byte[bufferSize];
			
			int bytesActuallyRead;

			bytesActuallyRead = data.Read (buffer, 0, bufferSize);
			while (bytesActuallyRead >= 0) {
				if (bytesActuallyRead > 0) {
					os.Write (buffer, 0, bytesActuallyRead);
				}
				bytesActuallyRead = data.Read (buffer, 0, bufferSize);
			}

			os.Write ("\r\n");
		}

		public void finish()
		{
			os.Write (delimiter + boundary + delimiter + "\r\n");
			os.Close ();
		}

		public string getResponse()
		{
			var httpResponse = request.GetResponse () as HttpWebResponse;
			using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
				var result = streamReader.ReadToEnd ();
				return result;
			}
		}

	}
}

