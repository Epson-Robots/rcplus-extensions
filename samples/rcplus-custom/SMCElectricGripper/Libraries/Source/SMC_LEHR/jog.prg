'============================================================
' JA: SMC LEHRシリーズ　JOGタスク
' JA: SMC LEHR series JOG task
'============================================================

#include "errcode.inc"
#include "min_max.inc"

Function JogTask
	
	Integer ret
	
	' JA: タスク番号保存
	' EN: Save task No.
	SMC_LEHR_jogtaskno_ = MyTask
	
	' JA: Jod動作ループ
	' EN: Jog loop
	Do
		
		Real pos
		Integer count
	
		' JA: Jogボタン押下待ち
		' EN: Wait for pressing Jog button
		Wait SMC_LEHR_jog_ <> 0
		
		' JA: 現在位置取得
		' EN: Get current position
		ret = SMC_LEHR_GetCurPos(ByRef pos)
		
		' JA: 5回ジョグしたら連続移動
		' EN: After 5 Jogs, moving continuously
		count = 5
		
		Select SMC_LEHR_jog_
			
			Case 1	' JA:閉 EN:Close

				Do
					pos = pos - 0.1
					If pos < POS_MIN Then pos = POS_MIN
					ret = SMC_LEHR_SetPos(11, pos)
					ret = SMC_LEHR_Execute(11, 0)

					count = count - 1

				Loop While SMC_LEHR_jog_ > 0 And count > 0
				
				If SMC_LEHR_jog_ > 0 And count = 0 Then
					ret = SMC_LEHR_SetPos(11, POS_MIN)
					ret = SMC_LEHR_Execute(11, 1)
				EndIf
				
			Case 2	' JA:開 EN:Open
								
				Do
					pos = pos + 0.1
					If pos > POS_MAX Then pos = POS_MAX
					ret = SMC_LEHR_SetPos(11, pos)
					ret = SMC_LEHR_Execute(11, 0)

					count = count - 1

				Loop While SMC_LEHR_jog_ > 0 And count > 0

				If SMC_LEHR_jog_ > 0 And count = 0 Then
					ret = SMC_LEHR_SetPos(11, POS_MAX)
					ret = SMC_LEHR_Execute(11, 1)
				EndIf

		Send
		
		If count = 0 Then
			Wait SMC_LEHR_jog_ = 0
			ret = SMC_LEHR_Abort
		EndIf
		
		Wait 0
	
	Loop
Fend
