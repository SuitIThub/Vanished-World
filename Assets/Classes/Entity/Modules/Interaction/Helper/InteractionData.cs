namespace Entity
{ 
    public class InteractionData<T> : IIDBase where T : IIDBase
    {
        public string id { get; }

        public InteractionExecuter executer;

        public T origin { get; }
        public T sender { get; }

        public KVStorage data { get; private set; }

        public string[] interactionKeys;

        public InteractionData(T origin, T sender, string[] interactionKeys, KVStorage data = null, InteractionExecuter executer = null)        
        {
            this.id = IdUtilities.id;
            this.origin = origin;
            this.sender = sender;
            this.data = data ?? new KVStorage();
            this.interactionKeys = interactionKeys;
            this.executer = executer;
        }

        public void cancelInteraction()
        {
            executer.cancelInteractionExtern(sender as IIDBase);
        }
    }
}