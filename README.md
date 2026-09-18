JUMP KID
Phiên bản: Unity 6000.4.4f
Devlog #1 (18/09/2026)
Thay đổi:
- Thêm file PopUpManager.cs, xử lý vấn đề hiển thị bảng PopUp mỗi lần Level Up.
- Điều chỉnh nhẹ hàm CheckAndUpdateLevel, ApplyLevelSkin, thêm AnimateFaceID để thêm animation cho FaceId và PopUp.
Bug:
- Khi hiển thị PopUp, đang gặp vấn đề hình ảnh sau khi Level Up hiển thị trước, rồi mới hiển thị hình ảnh trước khi Level Up.
- Đã thử đảo vị trí của oldSkin và newSkin nhưng không thay đổi -> vấn đề không nằm ở file PopUpManager.
Vấn đề khác:
- Khi hiển thị PopUp, vẫn có thể click ra bên ngoài PopUp hoặc ấn Space để tắt chế độ PopUp kể cả khi chưa chạy xong.
- Chưa biết vấn đề do đâu.
