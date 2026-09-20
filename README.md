# JUMP KID
# Phiên bản: Unity 6000.4.4f
## Devlog #1 (18/09/2026)
1. Thay đổi:
- Thêm file PopUpManager.cs, xử lý vấn đề hiển thị bảng PopUp mỗi lần Level Up.
- Điều chỉnh nhẹ hàm CheckAndUpdateLevel, ApplyLevelSkin, thêm AnimateFaceID để thêm animation cho FaceId và PopUp.
2. Bug:
- Khi hiển thị PopUp, đang gặp vấn đề hình ảnh sau khi Level Up hiển thị trước, rồi mới hiển thị hình ảnh trước khi Level Up.
- Đã thử đảo vị trí của oldSkin và newSkin nhưng không thay đổi -> vấn đề không nằm ở file PopUpManager.
3. Vấn đề khác:
- Khi hiển thị PopUp, vẫn có thể click ra bên ngoài PopUp hoặc ấn Space để tắt chế độ PopUp kể cả khi chưa chạy xong.
- Chưa biết vấn đề do đâu.
## Devlopd #2 (20/09/2026)
1. Thay đổi
- Thêm file GameInputManager.cs, xử lý vấn đề chặn input mỗi lần hiển thị quá trình Level Up.
- Điều chỉnh nhẹ hàm PlayerController, PopUpManager để đóng băng game tạm thời (màn hỉnh game lẫn bảng điều khiển) khi diễn ra quá trình Level Up.
- Xử lý Bug ở Devlog #1
2. Bug:
- Hiện không có bug.
3. Vấn đề khác:
- Chưa thêm animation khi hiển thị Pop Up.
- Dự định chỉnh sửa code để tắt FaceID khi diễn ra quá trình Level Up và hiển thị lại sau khi hoàn thành quá trình Level Up.
