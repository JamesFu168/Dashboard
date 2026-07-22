// 模擬 Users
const USERS = [
  { id: 1, name: 'Alex', role: '經理 (Card Owner)' },
  { id: 2, name: 'Bob', role: '工程師' },
  { id: 3, name: 'Claire', role: '設計師' },
  { id: 4, name: 'David', role: '普通同仁' }
];

// 初始 State
let currentUserId = 1; // 預設為 Alex
let currentView = 'org'; // 'org' 或 'personal'
let draggedCardId = null;

// Mock Cards Initial Data
let cards = [
  {
    id: 'card-1',
    title: '🚀 部署 .NET 10 Web API 至 Azure',
    desc: '設定 CI/CD Pipeline 並連結 Azure SQL Database 執行資料庫 Migration。',
    status: 1, // To Do
    scope: 1,  // Organization
    ownerId: 1, // Alex (經理)
    tasks: [
      { id: 't1-1', title: '編寫 DbContext 與 Entity Models', isCompleted: true, assigneeId: 2 },
      { id: 't1-2', title: '設定 Azure SQL 連線與密碼 KeyVault', isCompleted: false, assigneeId: 2 },
      { id: 't1-3', title: '測試 JWT 驗證與 Cors 政策', isCompleted: false, assigneeId: 1 }
    ]
  },
  {
    id: 'card-2',
    title: '🎨 設計 Angular 看板 UI 與拖拉互動模組',
    desc: '採用 Dark Mode 現代流線視覺，使用 Angular CDK DragDrop。',
    status: 2, // Doing
    scope: 1,  // Organization
    ownerId: 1, // Alex (經理)
    tasks: [
      { id: 't2-1', title: '設計 Figma 看板原型圖與 Mockup', isCompleted: true, assigneeId: 3 },
      { id: 't2-2', title: '切換 Personal / Org 視角頁籤元件', isCompleted: true, assigneeId: 3 },
      { id: 't2-3', title: '實作 cdkDragDisabled 非 Owner 鎖定防護', isCompleted: false, assigneeId: 2 }
    ]
  },
  {
    id: 'card-3',
    title: '📊 準備 Q3 部門營運與 KPI 報告',
    desc: '彙整專案進度與團隊預算執行率。',
    status: 0, // Plan
    scope: 0,  // Personal
    ownerId: 1, // Alex (經理的個人卡片)
    tasks: [
      { id: 't3-1', title: '收集各小組月度數據', isCompleted: false, assigneeId: 1 }
    ]
  },
  {
    id: 'card-4',
    title: '🔒 個人技術學習：Angular Signals 與 .NET 10 效能測試',
    desc: '研究最新的 Angular 18 Signal 響應式狀態管理與性能優化。',
    status: 2, // Doing
    scope: 0,  // Personal
    ownerId: 2, // Bob (Bob 的個人卡片)
    tasks: [
      { id: 't4-1', title: '閱讀官方 Benchmark 報告', isCompleted: true, assigneeId: 2 },
      { id: 't4-2', title: '撰寫測試 Demo 專案', isCompleted: false, assigneeId: 2 }
    ]
  },
  {
    id: 'card-5',
    title: '✅ 系統全模組單元測試與端到端驗證',
    desc: '確保 Card Owner 移動權限與 Task Assignee 修改權限運作無誤。',
    status: 3, // Done
    scope: 1,  // Organization
    ownerId: 1, // Alex
    tasks: [
      { id: 't5-1', title: '撰寫 xUnit 商業邏輯測試', isCompleted: true, assigneeId: 2 },
      { id: 't5-2', title: '執行權限阻擋情境驗證', isCompleted: true, assigneeId: 3 }
    ]
  }
];

// 初始化執行
document.addEventListener('DOMContentLoaded', () => {
  renderApp();
});

// 切換當前登入者
function changeCurrentUser(userIdStr) {
  currentUserId = parseInt(userIdStr);
  renderApp();
}

// 切換視角 (Org / Personal)
function switchView(viewMode) {
  currentView = viewMode;
  document.getElementById('btnViewOrg').classList.toggle('active', viewMode === 'org');
  document.getElementById('btnViewPersonal').classList.toggle('active', viewMode === 'personal');
  renderApp();
}

// 渲染應用程式
function renderApp() {
  const currentUser = USERS.find(u => u.id === currentUserId);
  updateInfoBanner(currentUser);
  renderKanbanBoard();
}

// 更新頂部權限與動態提示 Banner
function updateInfoBanner(user) {
  const bannerEl = document.getElementById('infoBanner');
  let viewName = currentView === 'org' ? '🏢 組織視角' : '🔒 個人視角';
  
  let bannerText = `當前身份：<strong>${user.name} (${user.role})</strong> ｜ 當前檢視：<strong>${viewName}</strong><br>`;
  
  if (currentView === 'org') {
    bannerText += `💡 <em>提示：您可以看到組織卡片。如果您是該卡片 Owner (Alex)，可以自由<b>移動列表</b>與管理；如果您只是 Task 被指派者，卡片將顯示 <b>🔒 唯讀標籤</b> 且<b>無法移動</b>，但可以<b>勾選指派給您的 Task</b>！</em>`;
  } else {
    bannerText += `💡 <em>提示：個人視角僅展示您創立的私人卡片。</em>`;
  }
  
  bannerEl.innerHTML = bannerText;
}

// 渲染看板與 4 個 Column
function renderKanbanBoard() {
  // 清空所有 Column
  for (let s = 0; s <= 3; s++) {
    document.getElementById(`column-${s}`).innerHTML = '';
    document.getElementById(`count-${s}`).innerText = '0';
  }

  // 根據 View Mode 過濾存取卡片 (ABAC / LINQ Logic 模擬)
  const accessibleCards = cards.filter(card => {
    if (currentView === 'personal') {
      // 個人視角：僅顯示屬於自己的個人卡片
      return card.scope === 0 && card.ownerId === currentUserId;
    } else {
      // 組織視角：顯示 Scope=Organization 且 (屬於部門 或 有 Task 指派給當前使用者)
      if (card.scope !== 1) return false;
      const isOwner = card.ownerId === currentUserId;
      const isTaskAssignee = card.tasks.some(t => t.assigneeId === currentUserId);
      return isOwner || isTaskAssignee || currentUserId === 1 || currentUserId === 4; 
    }
  });

  // 計算每個 Column 數量
  const counts = [0, 0, 0, 0];

  accessibleCards.forEach(card => {
    counts[card.status]++;
    const cardEl = createCardElement(card);
    document.getElementById(`column-${card.status}`).appendChild(cardEl);
  });

  // 更新計數器
  for (let s = 0; s <= 3; s++) {
    document.getElementById(`count-${s}`).innerText = counts[s];
  }
}

// 建立單一 Card HTML DOM Element
function createCardElement(card) {
  const isOwner = card.ownerId === currentUserId;
  const ownerUser = USERS.find(u => u.id === card.ownerId);

  const cardDiv = document.createElement('div');
  cardDiv.className = `card-item ${!isOwner ? 'is-readonly' : ''}`;
  cardDiv.id = card.id;
  
  // 權限關鍵：僅 Card Owner 設定可拖拉
  if (isOwner) {
    cardDiv.setAttribute('draggable', 'true');
    cardDiv.ondragstart = (e) => handleDragStart(e, card.id);
    cardDiv.ondragend = handleDragEnd;
  } else {
    cardDiv.setAttribute('draggable', 'false');
    cardDiv.title = '🔒 您不是這張卡片的擁有者，無法移動列表。';
  }

  // 計算 Tasks 完成進度
  const totalTasks = card.tasks.length;
  const completedTasks = card.tasks.filter(t => t.isCompleted).length;
  const progressPercent = totalTasks === 0 ? 0 : Math.round((completedTasks / totalTasks) * 100);

  // 渲染 Tasks HTML
  let tasksHtml = '';
  card.tasks.forEach(task => {
    const isAssignee = task.assigneeId === currentUserId;
    const canToggle = isOwner || isAssignee; // Owner 或 Assignee 才可以勾選
    const assigneeUser = USERS.find(u => u.id === task.assigneeId);

    tasksHtml += `
      <div class="task-row">
        <div class="task-left">
          <input type="checkbox"
                 ${task.isCompleted ? 'checked' : ''}
                 ${!canToggle ? 'disabled' : ''}
                 title="${!canToggle ? '非您負責的 Task，不可修改' : '勾選更新完成狀態'}"
                 onchange="toggleTaskStatus('${card.id}', '${task.id}')" />
          <span class="task-title ${task.isCompleted ? 'completed' : ''}">${task.title}</span>
        </div>
        ${assigneeUser ? `<span class="assignee-badge ${isAssignee ? 'is-me' : ''}">@${assigneeUser.name}</span>` : ''}
      </div>
    `;
  });

  cardDiv.innerHTML = `
    <div class="card-header-tags">
      <span class="badge-scope ${card.scope === 1 ? 'scope-org' : 'scope-personal'}">
        ${card.scope === 1 ? '🏢 組織' : '🔒 個人'}
      </span>
      ${!isOwner ? `<span class="readonly-lock">🔒 唯讀 (Task 參與者)</span>` : ''}
    </div>
    <div class="card-title">${card.title}</div>
    ${card.desc ? `<div class="card-desc">${card.desc}</div>` : ''}

    ${totalTasks > 0 ? `
      <div class="task-section">
        <div class="task-progress">
          <div class="progress-bar-bg">
            <div class="progress-bar-fill" style="width: ${progressPercent}%"></div>
          </div>
          <span class="progress-text">${completedTasks}/${totalTasks}</span>
        </div>
        <div class="task-list">
          ${tasksHtml}
        </div>
      </div>
    ` : ''}

    <div class="card-footer">
      <span>擁有者: <strong>${ownerUser ? ownerUser.name : 'Unknown'}</strong></span>
      <span>${totalTasks} 項 Task</span>
    </div>
  `;

  return cardDiv;
}

// Drag and Drop 處理邏輯
function handleDragStart(e, cardId) {
  draggedCardId = cardId;
  e.target.classList.add('dragging');
  e.dataTransfer.setData('text/plain', cardId);
}

function handleDragEnd(e) {
  e.target.classList.remove('dragging');
  draggedCardId = null;
}

function allowDrop(e) {
  e.preventDefault();
}

function handleDrop(e, targetStatus) {
  e.preventDefault();
  if (!draggedCardId) return;

  const card = cards.find(c => c.id === draggedCardId);
  if (!card) return;

  // 權限二次檢查 (防衛)
  if (card.ownerId !== currentUserId) {
    alert('⚠️ 權限拒絕：只有該卡片的擁有者 (Owner) 才能移動卡片列表狀態！');
    return;
  }

  // 更新卡片狀態
  card.status = targetStatus;
  renderKanbanBoard();
}

// 勾選 / 取消勾選 Task
function toggleTaskStatus(cardId, taskId) {
  const card = cards.find(c => c.id === cardId);
  if (!card) return;

  const task = card.tasks.find(t => t.id === taskId);
  if (!task) return;

  // 權限檢查
  const isOwner = card.ownerId === currentUserId;
  const isAssignee = task.assigneeId === currentUserId;

  if (!isOwner && !isAssignee) {
    alert('⚠️ 權限拒絕：您不是該 Task 的指派處理人，無法修改狀態！');
    renderKanbanBoard(); // 重置 UI
    return;
  }

  task.isCompleted = !task.isCompleted;
  renderKanbanBoard(); // 即時重新計算進度條
}

// Modal 相關操作
function openCreateCardModal() {
  document.getElementById('cardModal').classList.add('active');
  document.getElementById('newTaskList').innerHTML = '';
  addNewTaskInputRow(); // 預設帶一列 Task 輸入
}

function closeCreateCardModal() {
  document.getElementById('cardModal').classList.remove('active');
  document.getElementById('createCardForm').reset();
}

function addNewTaskInputRow() {
  const taskListDiv = document.getElementById('newTaskList');
  const rowDiv = document.createElement('div');
  rowDiv.className = 'new-task-row';
  
  let optionsHtml = USERS.map(u => `<option value="${u.id}">${u.name}</option>`).join('');

  rowDiv.innerHTML = `
    <input type="text" placeholder="細項 Task 名稱..." class="task-name-input" required />
    <select class="task-assignee-select" style="width: 130px;">
      ${optionsHtml}
    </select>
  `;
  taskListDiv.appendChild(rowDiv);
}

function handleCreateCard(e) {
  e.preventDefault();

  const title = document.getElementById('cardTitleInput').value.trim();
  const desc = document.getElementById('cardDescInput').value.trim();
  const scope = parseInt(document.getElementById('cardScopeInput').value);
  const status = parseInt(document.getElementById('cardStatusInput').value);

  // 蒐集新 Tasks
  const taskRows = document.querySelectorAll('.new-task-row');
  const newTasks = [];
  taskRows.forEach((row, index) => {
    const tName = row.querySelector('.task-name-input').value.trim();
    const tAssignee = parseInt(row.querySelector('.task-assignee-select').value);
    if (tName) {
      newTasks.push({
        id: `t-new-${Date.now()}-${index}`,
        title: tName,
        isCompleted: false,
        assigneeId: tAssignee
      });
    }
  });

  const newCard = {
    id: `card-${Date.now()}`,
    title: title,
    desc: desc,
    status: status,
    scope: scope,
    ownerId: currentUserId, // 建立者為 Owner
    tasks: newTasks
  };

  cards.push(newCard);
  closeCreateCardModal();
  renderKanbanBoard();
}
