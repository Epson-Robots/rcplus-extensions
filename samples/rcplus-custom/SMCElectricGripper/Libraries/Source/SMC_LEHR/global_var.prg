'||
'|| JA: グローバル変数
'|| EN: Global variables
'||

' JA: 関数戻り値
' EN: Function return value 
Global Static Integer SMC_LEHR_return_ ' JA: 最後に実行されたライブラリーファンクションの戻り値（ファンクションが実行中の時-1）EN: Return value of last executed library function (-1 for function running)

' JA: ByRef引数
' EN: ByRef arguments
Global Static UByte SMC_LEHR_mode_ 	' JA: モード設定値	EN: mode 
Global Static Real SMC_LEHR_pos_ 	' JA: 把持/位置決め位置設定値（単位:mm） EN: Gripping/Positioning Position (unit: mm)
Global Static Real SMC_LEHR_spd_ 	' JA: 把持/位置決め速度設定値（単位:mm/sec）EN: Gripping/Positioning Speed (unit: mm/sec)
Global Static Real SMC_LEHR_frc_ 	' JA: 把持/位置決め力設定値（単位:N）EN: Gripping/Positioning Force (unit: N)

' JA: ステータス
' EN: Status
Global Static UShort SMC_LEHR_sts_	' JA: ステータス（参照：マニュアル）EN: Status (refer to the manual)
Global Static Real SMC_LEHR_curpos_ ' JA: 現在位置（単位:mm） EN: Current position (unit: mm)
Global Static Real SMC_LEHR_curspd_ ' JA: 現在速度（単位:mm/sec）EN: Current speed (unit: mm/sec)
Global Static Real SMC_LEHR_curfrc_ ' JA: 現在の力（単位:N）EN: Current force (unit: N)
Global Static UByte SMC_LEHR_commtaskno_	' JA: 通信タスクのタスクNo EN: Task No. for communication task
Global Static UByte SMC_LEHR_servo_onoff_	' JA: サーボOff/On（0=Off、 1=On） EN: Servo Off/On (0=Off, 1=On)
Global Static UByte SMC_LEHR_running_		' JA: グリッパーが動作中（1=動作中、 0=停止） EN: Gripper working (1=working, 0=stopping)

' JA: UIからのJog操作
' EN: Jog process from the UI
Global Static UByte SMC_LEHR_jogtaskno_ ' JA: ジョグタスクのタスクNo EN: Task No. for Jog task
Global Static Integer SMC_LEHR_jog_ 	' JA: ジョグ 0:停止 1:開 2:閉 EN: Jog　0:Stop 1:Open 2: Close 

' JA: パラメータセーブ無効
' EN: Ignore parameter saving
Global Static Integer SMC_LEHR_disable_param_save_ 	' JA: パラメータセーブ無効化 EN: Ignore parameter saving

