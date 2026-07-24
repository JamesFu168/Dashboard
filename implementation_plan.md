# 小型部門看板系統開發執行計畫 (Implementation Plan)

本計畫為「小型部門看板系統」的開發執行計畫，包含後端 .NET 10 Web API、Azure SQL Database 以及前端 Angular 應用的建置與驗證流程。

---

## 階段一：後端基礎建設與資料庫 Entity 設計 (.NET 10 & EF Core)

- [x] **1.1 建立 .NET 10 Web API 專案骨架**
  - 初始化 ASP.NET Core Web API 專案 (`Dashboard.Api`)。
  - 配置 `appsettings.json` 與 connection string。
  - 導入 JWT Authentication（Access Token + Refresh Token 雙 Token 機制）與 .NET 10 原生 OpenAPI + Scalar UI。
  - 設定 CORS 政策，允許指定的前端網域存取。

- [x] **1.2 建立 EF Core Entities 與 DbContext**
  - 建立 `User`, `Department`, `Card`, `CardTask`, `RefreshToken` 實體。
  - `User` 包含 `PasswordHash` 欄位，密碼以 BCrypt 雜湊後儲存。
  - `Card` 新增 `IsDeleted` 與 `DeletedAt` 軟刪除欄位，並配置全域 Query Filter (`HasQueryFilter(c => !c.IsDeleted)`)。
  - 設定 Enum：`CardStatus` (`Plan`, `ToDo`, `Doing`, `Done`) 與 `CardScope` (`Personal`, `Organization`)。
  - 所有時間戳記欄位（`CreatedAt`/`UpdatedAt`/`DeletedAt`/Token 到期時間）統一透過 `DateTimeProvider.TaiwanNow` 取得台灣時間（UTC+8）。

- [x] **1.3 資料庫 Migration 與 Initial Seed Data**
  - 執行 EF Core Migration 建置資料庫 Schema。
  - 建立種子資料 (預設部門 Engineering/Product、測試使用者 Alex Owner / Sam Assignee、初始範例卡片與 Tasks)。

---

## 階段二：後端核心商業邏輯與 API 開發

- [x] **2.1 Auth API 與密碼驗證**
  - 實作 `POST /api/v1/auth/login`（Email + 密碼登入，BCrypt 驗證，成功後回傳 Access Token 與 Refresh Token）。
  - 實作 `POST /api/v1/auth/refresh`（驗證 Refresh Token 有效性，撤銷舊 Token 並換發新 Token）。
  - 實作 `POST /api/v1/auth/logout`（撤銷指定 Refresh Token）。

- [x] **2.2 Card API 與權限驗證 Policy (含軟刪除與回收桶)**
  - 實作 `GET /api/v1/cards`（根據視角過濾 `Personal` / `Organization` 與可見性）。
  - 實作 `POST /api/v1/cards`（建立新卡片）。
  - 實作 `PATCH /api/v1/cards/{id}`（編輯卡片內容，需要 Owner 權限與樂觀鎖驗證）。
  - 實作 `PUT /api/v1/cards/{id}/status`（調整欄位狀態與 SequenceOrder，僅 Card Owner 可操作）。
  - 實作 `DELETE /api/v1/cards/{id}`（標記軟刪除 `IsDeleted = true` 與 `DeletedAt = TaiwanNow`）。
  - 實作 `GET /api/v1/cards/trash`（取得當前使用者在回收桶中的軟刪除卡片列表）。
  - 實作 `POST /api/v1/cards/{id}/restore`（還原已被軟刪除的卡片）。
  - 實作 `DELETE /api/v1/cards/{id}/permanent`（永久從資料庫中刪除卡片）。

- [x] **2.3 Task API 與人員指派限制**
  - 實作 `POST /api/v1/cards/{cardId}/tasks`（僅 Card Owner 可新增 Task；若卡片為 Personal 則自動預設為 Card Owner）。
  - 實作 `PATCH /api/v1/tasks/{taskId}/toggle`（Card Owner 或 Task Assignee 可勾選完成狀態）。
  - 實作 `PUT /api/v1/tasks/{taskId}/assign`（變更 Task 指派人員；若卡片為 Personal 則限制禁止指派他人並回傳 `400 Bad Request`）。
  - 實作 `DELETE /api/v1/tasks/{taskId}`（僅 Card Owner 可刪除細項）。

- [x] **2.4 SignalR 即時溝通 Hub 建立**
  - 建立 `KanbanHub` 處理 `CardCreated`, `CardUpdated`, `CardMoved`, `CardDeleted`, `TaskUpdated` 即時廣播事件。
  - 提供 Group 訂閱機制 (`department:{departmentId}`)，針對 Owner 個人與同部門廣播。

- [x] **2.5 User 查詢 API**
  - 實作 `GET /api/v1/users?departmentId={id}`（提供前端取得部門成員清單以供 Task 指派）。

---

## 階段三：前端 Angular 看板系統開發

- [x] **3.1 專案建置與 UI 庫整合**
  - 建置 Angular 18+ 專案。
  - 導入 `@angular/cdk/drag-drop` 與自訂主題 CSS。
  - 建立身份驗證 `AuthInterceptor` 與 `KanbanStateService` (Angular Signals)。

- [x] **3.2 主看板頁面與 4 個 Column 元件**
  - 建立 `KanbanBoardPageComponent` 展現 **Plan**, **To Do**, **Doing**, **Done** 四個 Column。
  - 建立頂部選單：切換「個人視角 (Personal)」與「組織視角 (Organization)」，以及回收桶按鈕與 Count Badge。

- [x] **3.3 權限連動與 Drag-and-Drop 限制**
  - 卡片元件設定 `[cdkDragDisabled]="!isOwner(card)"`，阻擋非 Card Owner 拖移卡片。
  - 細項 Task 設定 Checkbox 停用狀態 `[disabled]="!canToggleTask(card, task)"`。

- [x] **3.4 卡片與 Task 管理對話框**
  - 建立 `CardFormDialogComponent` 進行卡片標題、內容、視角與到期日之新增與編輯。
  - 建立 `CreateTaskDialogComponent` 進行細項 Task 新增與部門成員指派選單。

- [x] **3.5 回收桶管理對話框**
  - 建立 `TrashDialogComponent` 展現被軟刪除的卡片列表，並提供「還原卡片」與「永久刪除」操作按鈕。

---

## 階段四：全系統整合、測試與部署驗證

- [x] **4.1 端到端整合測試 (E2E / Functional Test)**
  - 驗證場景 A：卡片擁有者移動 Plan -> Doing (成功，所有同部門使用者透過 SignalR 收到最新位置)。
  - 驗證場景 B：員工 A 看見指派給自己的組織卡片，試圖拖拉 (觸發鎖定阻擋)。
  - 驗證場景 C：員工 A 勾選指派給自己的 Task 為完成 (成功狀態更新)。
  - 驗證場景 D：卡片擁有者進行卡片軟刪除 -> 進入回收桶查看 -> 還原卡片 (卡片成功重現於看板)。
  - 驗證場景 E：個人卡片嘗試指派 Task 給其他同仁 (後端回傳 400 Bad Request 阻擋)。

- [x] **4.2 部署設定**
  - 設定 Azure SQL Database 並維護 SSDT `Dashboard.Database.sqlproj` 腳本。
  - 配置 `azure-pipelines-api.yml`、`azure-pipelines-db.yml` 與 `azure-pipelines-web.yml`。

---

## 📌 驗證標準與品質防線

1. **安全性驗證**：後端雙重驗證（前端停用拖曳與按鈕 + 後端 403 Forbidden / 400 Bad Request 政策防禦）。
2. **UI 體驗驗證**：卡片拖拉順暢，回收桶還原與軟刪除機制回饋明確。
3. **資料一致性**：Task 勾選狀態變更不影響 Card 狀態，Card 刪除採用 Soft Delete 標記並保留歷史資訊；永久刪除時 Cascade 刪除 Tasks。
