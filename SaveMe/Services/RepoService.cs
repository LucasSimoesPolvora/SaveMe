public class RepoService
{
    public static void InitRepo()
    {
        DirectoryInfo info = new(Directory.GetCurrentDirectory());
        foreach(var file in info.GetDirectories()){
            if(file.Name == ".sm"){
                Console.WriteLine("Repository already exists...\n" + 
                                "This will delete all existing snapshots and data in the repository.\n" + 
                                "Would you like to overwrite it? (y/n)");
                if(Console.ReadKey().KeyChar == 'y'){
                    Directory.Delete(Directory.GetCurrentDirectory() + "\\.sm", true);
                    CreateRepo();
                } else{
                    Console.WriteLine("\nInitialization cancelled");
                    return;
                }
            }
        }
        CreateRepo();
        Console.WriteLine("\nRepository created successfully.");
    }
    public static void CreateRepo(){
        try{
            Directory.CreateDirectory(".sm");
            _ = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\.sm")
            {
                Attributes = FileAttributes.Hidden
            };
            
        } catch(Exception ex){
            Console.WriteLine($"Error creating repository: {ex.Message}");
        }
    }
}