using System;

namespace Estrella.Util
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ServerModuleAttribute : Attribute
    {
        private readonly InitializationStage stageInternal;

        public ServerModuleAttribute(InitializationStage initializationStage)
        {
            stageInternal = initializationStage;
        }

        public InitializationStage InitializationStage
        {
            get { return stageInternal; }
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class InitializerMethodAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CleanUpMethodAttribute : Attribute
    {
    }
}