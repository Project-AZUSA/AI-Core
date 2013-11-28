AI 引擎範本

使用方法:

一般情況下只需要修改 Initialize() 和 Process(msg) 的內容。 Initialize() 負責引擎的初始化, Process(msg) 則是將從所有輸入端收到的信息進行整合處理, 回傳輸出。

resolution 是 AI 的反應時間, 也就是說 Process 會每隔多久執行一次。
