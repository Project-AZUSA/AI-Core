AI 引擎範本

使用方法:

一般情況下只需要修改 Initialize() 和 Process(msg) 的內容。 Initialize() 負責引擎的初始化, Process(msg) 則是將從所有輸入端收到的信息進行整合處理, 再輸出指令。

resolution 是 AI 的反應時間, 也就是說 Process 會每隔多久執行一次。

程序使用到 libzmq.dll 和 clrzmq.dll , 發行時要一併發行
