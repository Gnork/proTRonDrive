//Christoph Jansen

using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Net;
using System.Text;

public class PhpConnect : MonoBehaviour {
	
	//Connect to webserver and send string to php script
	public static void Send(string postData, bool isAuto)
	{
		string url;
		if(isAuto)
			url = "http://protron.gnork.org/writeautodb.php";
		else
			url = "http://protron.gnork.org/writemanudb.php";
		byte[] byteArray = Encoding.UTF8.GetBytes (postData);
        WebRequest request = WebRequest.Create (url);
        request.Method = "POST";    
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = byteArray.Length;
        Stream dataStream = request.GetRequestStream ();
        dataStream.Write (byteArray, 0, byteArray.Length);
        dataStream.Close ();
	}
	
	//Connect to webserver and send request to read string from php script
	public static string Read()
	{
        WebRequest request = WebRequest.Create ("http://protron.gnork.org/readdb.php");
        request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
	 	WebResponse response = request.GetResponse ();
        Stream dataStream = response.GetResponseStream ();
        StreamReader reader = new StreamReader (dataStream);
        string responseFromServer = reader.ReadToEnd ();
        reader.Close ();
        dataStream.Close ();
        response.Close ();
	
		return responseFromServer;
	}
}
