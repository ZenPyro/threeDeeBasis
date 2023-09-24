using Godot;
using System;
using Newtonsoft.Json;

public class saveLoadJSON : Control
{
    private const String SAVE_PATH = "user://save.json";
    private const String SAVE_PASS = "passTest";
    private static DataModel _data = new DataModel();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ReadSaveFile();
    }

    public static void Save(){
        WriteSaveFile();
    }

    public void ReadSaveFile(){
        String jsonString = null;
        File saveFile = OpenSaveFile(File.ModeFlags.Read);

        if(saveFile != null){
            jsonString = saveFile.GetLine();

            //`try` is if we already have a `saveFile`
            try{
                _data = Deserialize(jsonString);
                GD.Print("Saved Correctly");
            }
            //`catch` is if we do not have a `saveFile` made yet and need to make a new one ->
            //-> by making a new `DataModel` instance
            catch(Exception e){
                _data = new DataModel();
                GD.Print("Saving Incorrectly");
                GD.Print(e);
            }

            saveFile.Close();
        }
    }

    private static File OpenSaveFile(File.ModeFlags flag = File.ModeFlags.Read){
        //IMPORTANT NOTE: The Class type `File` is a Class type used to permanetly store data ->
        //-> into the user device's file system and to read from it.
        //NOTE: This is a `Godot.File` Class
        File saveFile = new File();
        Error err = saveFile.OpenEncryptedWithPass(SAVE_PATH, flag, SAVE_PASS);

        if(err == 0){
            return saveFile;
        }
        return null;
    }

    private static DataModel Deserialize(String jsonString){
        return JsonConvert.DeserializeObject<DataModel>(jsonString);
    }

    private static void WriteSaveFile(){
        File saveFile = OpenSaveFile(File.ModeFlags.Write);

        if(saveFile != null){
            String jsonString = JsonConvert.SerializeObject(_data);

            saveFile.StoreLine(jsonString);

            saveFile.Close();
        }
    }

    //Below are many functions to help out with the data manipulation
    public static void AddMob(int count){
        _data.mobCount = count;
    }
    public static int GetMob(){
        return _data.mobCount;
    }
}
