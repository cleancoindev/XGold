namespace Expload {

    using Pravda;
    using System;

    [Program]
    public class GameToken {
        public static void Main() { }

        private Mapping<Bytes, UInt32> Balance = 
            new Mapping<Bytes, UInt32>();

        private Mapping<Bytes, int> WhiteList =
            new Mapping<Bytes, int>();
        
        // Gives amount of GameTokens to recipient.
        public void Give(Bytes recipient, UInt32 amount) {
            assertIsOwner();
            UInt32 lastBalance = Balance.GetOrDefault(recipient, 0);
            UInt32 newBalance = lastBalance + amount;
            Balance[recipient] = newBalance;
        }

        // Remove amount of GameTokens from balance of address.
        public void Burn(Bytes address, UInt32 amount) {
            assertIsOwner();
            UInt32 balance = Balance.GetOrDefault(address, 0);
            if (balance >= amount) {
                Balance[address] = balance - amount;
            } else {
                Error.Throw("Not enough funds");
            }
        }

        // Add address to white list
        public void WhiteListAdd(Bytes address) {
            assertIsOwner();
            WhiteList[address] = 1;
        }

        // Remove address from white list.
        public void WhiteListRemove(Bytes address) {
            assertIsOwner();
            WhiteList[address] = 0;
        }

        public UInt32 MyBalance()
        {
            Bytes sender = Info.Sender();
            UInt32 senderBalance = Balance.GetOrDefault(sender, 0);
            return senderBalance;
        }

        // Send GameTokens from transaction Sender to recipient.
        // Recipient should be present in the Game Developers white list.
        public void Spend(Bytes recipient, UInt32 amount) {
            if (WhiteListCheck(recipient)) {
                Bytes sender = Info.Sender();
                UInt32 senderBalance = Balance.GetOrDefault(sender, 0);
                UInt32 recipientBalance = Balance.GetOrDefault(recipient, 0);
                if (senderBalance >= amount) {
                    Balance[sender] = senderBalance - amount;
                    Balance[recipient] = recipientBalance + amount;
                    Log.Event("GameToken:Spend", new SpendEventData(recipient, amount));
                } else {
                    Error.Throw("GameTokenError: Not enough funds for Spend operation");
                }
            } else Error.Throw("Operation denied");
        }
        
        //// Private methods

        // Check address is white listed/
        private bool WhiteListCheck(Bytes address)
        {
            return WhiteList.GetOrDefault(address, 0) == 1;
        }

        private void assertIsOwner()
        {
            if (Info.Sender() != Info.ProgramAddress())
            {
                Error.Throw("Only owner of the program can do that.");
            }
        }
    }

    class SpendEventData {
        public SpendEventData(Bytes recipient, UInt32 amount) {
            this.recipient = recipient;
            this.amount = amount;
        }
        public UInt32 amount;
        public Bytes recipient;
    }
}
