using System.Reflection;
using System.Text.Json;

namespace ActionDispatcher
{
    public record class Parameter(string Type, JsonElement Value);
    public record class Action(string Path, string Type, string Name, List<Parameter> Parameters);
    public record class TaskQueue(List<Action> Actions);
    public class Dispatcher
    {
        public async Task BuildAsync()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            baseDirectory = Path.Combine(baseDirectory, "Plugins", "Commands");
            string[] files = Directory.GetFiles(baseDirectory, "*.dll");
            var task = new List<Action>();
            foreach (string item in files)
            {
                Assembly assembly = Assembly.LoadFrom(item);
                Type[] exportedTypes = assembly.GetExportedTypes();
                foreach (var type in exportedTypes)
                {
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    foreach (MethodInfo method in methods)
                    {
                        var parameters = new List<Parameter>();
                        ParameterInfo[] ps = method.GetParameters();
                        foreach (var p in ps)
                        {
                            var parameterValue = JsonSerializer.Serialize(Activator.CreateInstance(p.ParameterType));
                            var jsonElement = JsonSerializer.Deserialize<JsonElement>(parameterValue);
                            parameters.Add(new Parameter(p.ParameterType.AssemblyQualifiedName!, jsonElement));
                        }
                        var action = new Action(Path.Combine("Plugins", "Commands", Path.GetFileName(item)), $"{type.FullName}", method.Name, parameters);
                        task.Add(action);
                    }
                }
            }
            var taskQ = new TaskQueue(task);
            await using FileStream createStream = File.Create("Test1.json");
            await JsonSerializer.SerializeAsync(createStream, taskQ);
        }

        public async Task RunAsync(string file)
        {
            await using FileStream openStream = File.OpenRead(file);
            try
            {
                var task = await JsonSerializer.DeserializeAsync<TaskQueue>(openStream);
                if (task is null) return;
                foreach (var action in task.Actions)
                {
                    var assembly = Assembly.LoadFrom(action.Path);
                    if (assembly is null) continue;
                    var myType = assembly.GetType(action.Type);
                    if (myType is null) continue;
                    var methodInfo = myType.GetMethod(action.Name);
                    if (methodInfo is null) continue;
                    var instance = Activator.CreateInstance(myType);
                    if (instance is null) continue;
                    var parameters = new List<object>();
                    foreach (var paramInfo in action.Parameters)
                    {
                        var paramType = Type.GetType(paramInfo.Type);
                        if (paramType is null) continue;
                        var paramValue = JsonSerializer.Deserialize(paramInfo.Value.GetRawText(), paramType);
                        if (paramValue is null) continue;
                        parameters.Add(paramValue);
                    }
                    var a = methodInfo.Invoke(instance, [.. parameters]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
