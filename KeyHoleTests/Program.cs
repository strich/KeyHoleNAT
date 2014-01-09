using KeyHole;

namespace KeyHoleTests {
	class Program {
		static void Main(string[] args) {
			ConnectionManager cm = new ConnectionManager();
			cm.CreateServer();

            UPNPOptions uo = new UPNPOptions {Enabled = true, TimeOut = 12345};

		    KeyHole.KeyHole kh = new KeyHole.KeyHole();
            

			while(true) {
				
			}

		}
	}
}
