'||
'|| JA: Modbus定数
'|| EN: Modbus constants
'||
#define MODBUS_ADDR 1	 				' JA: スレーブID EN: Slave ID
#define MBUS_FUNC_READ &H03				' JA: 保持レジスタ読出し EN: Read holding register
#define MBUS_FUNC_WRITE &H05	 		' JA: 単一コイル書込み EN: Write Single coil
#define MBUS_FUNC_WRITERANGE &H10		' JA: 複数保持レジスタ書込み EN: Write multiple holding register

'||
'|| JA: Modbusレジスタ定数
'|| EN: Modbus register constants
'||
#define MODBUS_ON &HFF00	' ON
#define MODBUS_OFF &H0000	' OFF

'||
'|| JA: 待ち時間
'|| EN: Waiting time
'||
#define MODBUS_RECV_WAIT 0.02	' JA: コマンド送信してから受信までの間隔 EN: Interval from sending to recieving commands
#define SAVE_WAIT 0.3			' JA: セーブコマンドの待ち時間 EN: Interval for save command completion
