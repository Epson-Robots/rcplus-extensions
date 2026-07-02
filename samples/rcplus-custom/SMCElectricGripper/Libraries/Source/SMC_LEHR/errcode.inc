'||
'|| JA: エラーコード
'|| EN: Error codes
'||
#define NOERR 0 			' JA: 正常 EN: No error
#define ERR_ARGUMENT 1		' JA: 引数が範囲外または不正な値 EN: Argument out of range or invalid value
#define ERR_COM_OPEN 2		' JA: COMポートをOpenできない EN: COM port cannot open 
#define ERR_COMMUNICATION 3	' JA: グリッパとの通信に失敗 EN: Faild to communicate with the gripper 
#define ERR_SERVO_OFF 4 	' JA: サーボがOFFのためレシピを実行できない EN: Cannot run the recipe due to servo off


'||
'|| JA: ステータス、ワーニング （マニュアル参照）
'|| EN: Status, warnings (Refer to the manual)
'||
#define ALARM_NONE 0
#define ALARM_OVERLOAD 100
#define ALARM_OVERCURRENT 101
#define ALARM_OVERTEMP 102
#define ALARM_OVERVOLT 103
#define ALARM_UNDERVOLT 104
#define ALARM_OVERFLOW 105
#define WARNING_GRIPFAIL 200
#define WARNING_WORKLOST 201
#define WARNING_OVERLOAD 202
#define WARNING_TEMP 203
