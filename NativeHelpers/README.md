This project is a native version of the Rijndael cipher (AES).

AES is used in the key transformation algorithm of KeePass. I found that using WinRT's AES APIs in a loop were too slow, so that functionality is delegated to this helper library.