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

	/*private void LoadOptions()
	{
		string fullpath = this.GetFilePath();

		if ( File.Exists( fullpath ) == false )
		{
			this.CreateDefaultFile();
		}

		XmlDocument doc = new XmlDocument();
		try 
		{
			doc.Load( fullpath );
		}
		catch ( FileNotFoundException _fileNotFound )
		{
			Debug.LogError( "File was not created: " + _fileNotFound.FileName );
			return;
		}

		/*XmlReader reader = XmlReader.Create( fullpath );

		while ( reader.Read() )
		{
			switch ( reader.NodeType )
			{
			case XmlNodeType.Element:
			{
				Debug.Log( reader.Name );
				break;
			}
			case XmlNodeType.Text:
			{
				Debug.Log( reader.Value );
				break;
			}
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.ProcessingInstruction:
			{
				Debug.Log( reader.Name + ":" + reader.Value );
				break;
			}
			case XmlNodeType.Comment:
			{
				Debug.Log( reader.Value );
				break;
			}
			case XmlNodeType.EndElement:
			{
				Debug.Log( "End" );
				break;
			}
			}
		}
		Debug.Log ( reader.Read() );* /

		if ( doc.FirstChild == null )
		{
			Debug.LogError( "Error opening options file. ");
			return;
		}

		/*XmlNode nameNode = doc.FirstChild.SelectSingleNode( "name" );
		if ( nameNode != null )
		{
			this.playerName = nameNode.Value;
		}
#if UNITY_EDITOR
		else
		{
			Debug.LogError( "Cannot find name node in options file." );
		}
#endif* /
		//Debug.Log( Application.dataPath );
	}*/
	
	/*private void CreateDefaultFile()
	{
		FileStream stream = new FileStream( this.GetFilePath(), FileMode.CreateNew );
		XmlWriter writer = XmlWriter.Create ( stream );

		Debug.Log( "Creating new file." );

		writer.WriteStartDocument();
		writer.WriteStartElement( "save" );
		writer.WriteElementString( "name", this.playerName );
		writer.WriteEndElement();
		writer.WriteEndDocument();
		writer.Flush();
		writer.Close();
		stream.Close();
	}*/

	private string GetFilePath()
	{
		return Application.dataPath + "\\" + this.filename;
	}
}
