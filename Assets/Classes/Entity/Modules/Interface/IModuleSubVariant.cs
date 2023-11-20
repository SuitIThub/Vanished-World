using static Entity.IModule;

namespace Entity
{
    public interface IModuleSubVariant
    {
        /// <summary>
        /// copies the Module
        /// </summary>
        /// <returns>a Copy</returns>
        public IModule Copy();

        /// <summary>
        /// saves and returns this module
        /// </summary>
        /// <returns>the <see cref="Save"/> containing this module-data</returns>
        public IModuleSave SaveModule();

        /// <summary>
        /// loads the data from <paramref name="save"/> and inserts it into this Module
        /// </summary>
        /// <param name="save">the <see cref="Save"/> containing the save-data</param>
        /// <returns>
        ///     <list type="bullet|number|table">
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.SUCCESS"/> (0)</term>
        ///             <description>Successfully loaded save-data into module</description>
        ///         </item>
        ///         <item>
        ///             <term><see cref="ReturnCode.codeEnum.INVALID_VALUE"/> (102)</term>
        ///             <description><see cref="Save.moduleType"/> does not fit the <see cref="ModuleType"/> of this module</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public ReturnCode LoadModule(IModuleSave save);

        /// <summary>
        /// destroys the contents of this Module
        /// </summary>
        public void Destroy();

        /// <summary>
        /// initializes delegate-methods used for access of different module types
        /// </summary>
        public void IntegrateMethods();
    }
}