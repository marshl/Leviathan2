using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

public class Options
{
	public string playerName = "DefaultName";
};

public class PlayerOptions : MonoBehaviour
{
	// Instance
	public static PlayerOptions instance;

	// Editor Variables
	public string filename = "options.xml";

	// Public Variables
	public Options options;

	private void Awake()
	{
		this.options = new Options();
		this.LoadOptions();
		DontDestroyOnLoad( this );
	}
	
	private void LoadOptions()
	{
		XmlSerializer serialiser = new XmlSerializer( typeof(Options) );
		FileStream stream;
		try 
		{
			stream = new FileStream( this.GetFilePath(), FileMode.Open );
		}
		catch
		{
			this.WriteToFile();
			stream = new FileStream( this.GetFilePath(), FileMode.Open );
		}

		XmlReader reader = XmlReader.Create( stream );
		this.options = (Options)serialiser.Deserialize( reader );
		stream.Close();
	}

	private void WriteToFile()
	{
		XmlSerializer serialiser = new XmlSerializer( typeof(Options) );
		FileStream stream = new FileStream( this.GetFilePath(), FileMode.CreateNew );
		if ( stream == null )
		{
			Debug.LogError( "Error creating default options file." );
			return;
		}
		serialiser.Serialize( stream, this.options );
		stream.Close();
	}

	private string GetFilePath()
	{
		return Application.dataPath + "\\" + this.filename;
	}
}
