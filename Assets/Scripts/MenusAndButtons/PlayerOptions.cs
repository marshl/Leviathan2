using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

[System.Serializable]
public class Options
{
	public string playerName = "DefaultName";

	public Options()
	{
		string[] names = { "Huey", "Duey", "Louie", };
		System.Random newRand = new System.Random();

		this.playerName = names[ newRand.Next() % names.Length ];
	}
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
		instance = this;
		this.options = new Options();
		this.LoadOptions();
		DontDestroyOnLoad( this );
	}

	private void OnApplicationQuit()
	{
		this.WriteToFile();
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
			if ( !this.WriteToFile() )
			{
				Debug.Log( "An error occurred trying to recitify the previous error" );
				return;
			}
			stream = new FileStream( this.GetFilePath(), FileMode.Open );
		}

		XmlReader reader = XmlReader.Create( stream );
		try
		{
			this.options = serialiser.Deserialize( reader ) as Options;
			stream.Close();
		}
		catch ( XmlException _exception )
		{ 
			DebugConsole.Error( _exception.ToString() );
			stream.Close();
			this.options = new Options();
			this.WriteToFile();
			this.LoadOptions();
		}

		if ( this.options == null )
		{
			this.options = new Options();
			DebugConsole.Error( "error loading options file, starting fresh" );
		}
	}

	private bool WriteToFile()
	{
		XmlSerializer serialiser = new XmlSerializer( typeof(Options) );

		FileInfo fileInfo = new FileInfo( this.GetFilePath() ); 
		if ( fileInfo.Exists ) 
		{
			fileInfo.Delete(); 
		} 

		FileStream stream = new FileStream( this.GetFilePath(), FileMode.OpenOrCreate );
		if ( stream == null )
		{
			DebugConsole.Error( "Error creating default options file." );
			return false;
		}
		serialiser.Serialize( stream, this.options );
		stream.Close();
		return true;
	}

	private string GetFilePath()
	{
		return Application.dataPath + "\\" + this.filename;
	}


}
