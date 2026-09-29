const state = {
  token: sessionStorage.getItem("demoToken"),
  productId: 100
};

const views = ["dashboard", "configure", "product", "api", "cart", "settings", "validation", "architecture"];
const titles = {
  dashboard: "Admin Dashboard",
  configure: "Product Configuration",
  product: "Customer Product Page",
  api: "Server Side Calculation API",
  cart: "Cart",
  settings: "Settings",
  validation: "Validation Demo",
  architecture: "Plugin Architecture"
};

const sampleRequest = {
  productId: 100,
  length: 5,
  width: 4
};

let activeView = "";
let primaryUnit = "pack";
let linkedUnit = "bucket";

document.addEventListener("DOMContentLoaded", () => {
  document.getElementById("login-form").addEventListener("submit", onLogin);
  document.getElementById("logout").addEventListener("click", onLogout);
  document.getElementById("nav").addEventListener("click", onNav);
  document.getElementById("config-form").addEventListener("submit", onSaveConfig);
  document.getElementById("config-linked").addEventListener("change", syncRuleInputs);
  document.getElementById("config-linked-qty").addEventListener("input", syncRulePreview);
  document.getElementById("config-linked-per").addEventListener("input", syncRulePreview);
  document.getElementById("calc-form").addEventListener("submit", (event) => event.preventDefault());
  document.getElementById("btn-calculate").addEventListener("click", () => runCalculation(false));
  document.getElementById("btn-add").addEventListener("click", () => runCalculation(true));
  document.getElementById("product-picker").addEventListener("change", (event) => {
    state.productId = Number(event.target.value);
    loadProduct();
  });
  document.getElementById("api-form").addEventListener("submit", (event) => {
    event.preventDefault();
    sendApi(false);
  });
  document.getElementById("api-tamper").addEventListener("click", () => sendApi(true));
  document.getElementById("settings-form").addEventListener("submit", onSaveSettings);
  document.getElementById("settings-enable").addEventListener("change", syncEnableStatus);
  document.getElementById("settings-reset").addEventListener("click", onReset);
  document.getElementById("val-form").addEventListener("submit", (event) => {
    event.preventDefault();
    runValidation();
  });
  window.addEventListener("hashchange", () => {
    const name = location.hash.replace("#", "");
    if (state.token && name !== activeView && views.includes(name))
      showView(name);
  });

  if (state.token)
    enterApp();
});

async function onLogin(event) {
  event.preventDefault();
  const error = document.getElementById("login-error");
  error.hidden = true;
  try {
    const data = await api("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({
        username: document.getElementById("username").value.trim(),
        password: document.getElementById("password").value
      })
    });
    state.token = data.token;
    sessionStorage.setItem("demoToken", data.token);
    enterApp();
  } catch (err) {
    error.hidden = false;
    error.textContent = err.data?.detail || err.data?.error || "Could not sign in.";
  }
}

async function onLogout() {
  try {
    await api("/api/auth/logout", { method: "POST" });
  } catch {
    // The local session is cleared either way.
  }
  signOut();
}

function signOut() {
  sessionStorage.removeItem("demoToken");
  state.token = null;
  activeView = "";
  document.getElementById("app").hidden = true;
  document.getElementById("login-screen").hidden = false;
}

function enterApp() {
  document.getElementById("signed-in").textContent = "demo@nopdemo.com";
  document.getElementById("login-screen").hidden = true;
  document.getElementById("app").hidden = false;
  document.getElementById("api-request").value = JSON.stringify(sampleRequest, null, 2);
  const requested = location.hash.replace("#", "");
  showView(views.includes(requested) ? requested : "dashboard");
}

function onNav(event) {
  const button = event.target.closest(".nav-btn");
  if (!button)
    return;
  if (button.dataset.view === "product")
    state.productId = 100;
  showView(button.dataset.view);
}

function showView(name) {
  activeView = name;
  document.querySelectorAll(".view").forEach((view) => {
    view.hidden = view.id !== `view-${name}`;
  });
  document.querySelectorAll(".nav-btn").forEach((button) => {
    button.classList.toggle("active", button.dataset.view === name);
  });
  document.getElementById("view-title").textContent = titles[name];
  if (location.hash !== `#${name}`)
    location.hash = name;
  loadView(name);
}

function loadView(name) {
  const loaders = {
    dashboard: loadDashboard,
    configure: loadConfiguration,
    product: loadProduct,
    api: () => {},
    cart: loadCart,
    settings: loadSettings,
    validation: runValidation,
    architecture: () => {}
  };
  Promise.resolve(loaders[name]()).catch(() => {});
}

async function api(path, options = {}) {
  const response = await fetch(path, {
    method: options.method || "GET",
    headers: {
      "Content-Type": "application/json",
      ...(state.token ? { Authorization: `Bearer ${state.token}` } : {})
    },
    body: options.body
  });
  const data = await response.json().catch(() => ({}));
  if (response.status === 401 && path !== "/api/auth/login") {
    signOut();
    const error = new Error("Unauthorized");
    error.data = data;
    throw error;
  }
  if (!response.ok) {
    const error = new Error(data.error || "Request failed");
    error.data = data;
    error.status = response.status;
    throw error;
  }
  return data;
}

async function loadDashboard() {
  const products = await api("/api/products");
  const body = document.getElementById("product-rows");
  body.innerHTML = products.map((product) => `
    <tr>
      <td>${escapeHtml(product.name)}</td>
      <td>${escapeHtml(product.type)}</td>
      <td><span class="badge ${escapeHtml(product.calculator.toLowerCase())}">${escapeHtml(product.calculator)}</span></td>
      <td><button type="button" class="linkish" data-configure="${product.id}">Configure</button></td>
    </tr>
  `).join("");
  body.querySelectorAll("[data-configure]").forEach((button) => {
    button.addEventListener("click", () => {
      state.productId = Number(button.dataset.configure);
      showView("configure");
    });
  });
}

async function loadConfiguration() {
  const [product, calculators, products] = await Promise.all([
    api(`/api/products/${state.productId}`),
    api("/api/calculators"),
    api("/api/products")
  ]);
  const accessory = product.type === "Accessory";
  document.getElementById("config-accessory").hidden = !accessory;
  document.getElementById("config-form").hidden = accessory;
  document.getElementById("accessory-name").textContent = product.name;
  if (accessory)
    return;

  document.getElementById("config-name").textContent = product.name;
  document.getElementById("config-id").textContent = `Product id ${product.id}`;
  const calculator = document.getElementById("config-calculator");
  calculator.innerHTML = calculators.map((item) =>
    `<option value="${escapeHtml(item.key)}">${escapeHtml(item.displayName)}</option>`
  ).join("");
  calculator.value = product.calculatorKey || calculators[0]?.key || "";
  document.getElementById("config-coverage").value = product.coveragePerPack;
  document.getElementById("config-min").value = product.minimumQuantity;
  document.getElementById("config-enable").checked = product.enableCalculator;

  const linked = document.getElementById("config-linked");
  const accessories = products.filter((item) => item.type === "Accessory");
  linked.innerHTML = `<option value="">None</option>` + accessories.map((item) =>
    `<option value="${item.id}">${escapeHtml(item.name)}</option>`
  ).join("");
  linked.value = product.linkedProductId ? String(product.linkedProductId) : "";
  document.getElementById("config-linked-qty").value = product.linkedQuantity || 1;
  document.getElementById("config-linked-per").value = product.linkedPerQuantity || 10;
  document.getElementById("config-message").hidden = true;
  primaryUnit = product.unit || "pack";
  linkedUnit = product.linkedProductUnit || "bucket";
  syncRuleInputs();
}

async function syncRuleInputs() {
  const linkedId = document.getElementById("config-linked").value;
  document.getElementById("config-rule").hidden = linkedId === "";
  if (linkedId) {
    const linked = await api(`/api/products/${linkedId}`);
    linkedUnit = linked.unit || "bucket";
  }
  syncRulePreview();
}

function syncRulePreview() {
  const linkedId = document.getElementById("config-linked").value;
  const preview = document.getElementById("config-rule-preview");
  if (!linkedId) {
    preview.textContent = "";
    return;
  }
  const qty = Number(document.getElementById("config-linked-qty").value || 1);
  const per = Number(document.getElementById("config-linked-per").value || 10);
  document.getElementById("config-linked-unit").textContent = plural(linkedUnit, qty);
  document.getElementById("config-primary-unit").textContent = plural(primaryUnit, per);
  preview.textContent = `${qty} ${plural(linkedUnit, qty)} per ${per} ${plural(primaryUnit, per)}`;
}

async function onSaveConfig(event) {
  event.preventDefault();
  const message = document.getElementById("config-message");
  const linkedValue = document.getElementById("config-linked").value;
  const body = {
    calculatorKey: document.getElementById("config-calculator").value,
    coveragePerPack: Number(document.getElementById("config-coverage").value),
    minimumQuantity: Number(document.getElementById("config-min").value),
    enableCalculator: document.getElementById("config-enable").checked,
    linkedProductId: linkedValue ? Number(linkedValue) : null,
    linkedQuantity: Number(document.getElementById("config-linked-qty").value),
    linkedPerQuantity: Number(document.getElementById("config-linked-per").value)
  };
  try {
    const result = await api(`/api/products/${state.productId}/configuration`, {
      method: "PUT",
      body: JSON.stringify(body)
    });
    message.hidden = false;
    message.classList.remove("error");
    message.textContent = result.message + " The product page reads these values from the server.";
  } catch (err) {
    message.hidden = false;
    message.classList.add("error");
    message.textContent = err.data?.error || "Could not save configuration.";
  }
}

async function loadProduct() {
  const [product, settings] = await Promise.all([
    api(`/api/products/${state.productId}`),
    api("/api/settings")
  ]);
  const picker = document.getElementById("product-picker");
  if (!picker.options.length) {
    const products = await api("/api/products");
    picker.innerHTML = products
      .filter((item) => item.type !== "Accessory")
      .map((item) => `<option value="${item.id}">${escapeHtml(item.name)}</option>`)
      .join("");
  }
  picker.value = String(product.id);

  const storePicker = document.getElementById("store-picker");
  const previousStore = storePicker.value;
  storePicker.innerHTML = settings.stores.map((store) =>
    `<option value="${escapeHtml(store.id)}">${escapeHtml(store.name)}</option>`
  ).join("");
  storePicker.value = previousStore && [...storePicker.options].some((option) => option.value === previousStore)
    ? previousStore
    : "uk";

  const calculable = product.type !== "Accessory" && product.enableCalculator;
  document.getElementById("product-layout").hidden = !calculable;
  const disabled = document.getElementById("product-disabled");
  disabled.hidden = calculable;
  if (!calculable) {
    disabled.textContent = "Calculator is disabled for this product. Turn it on from Product Configuration.";
    return;
  }

  document.getElementById("product-image").src = product.calculatorKey === "wall-panel"
    ? "images/wall-panel.svg"
    : "images/oak-floor.svg";
  document.getElementById("product-image").alt = product.name;
  document.getElementById("product-type").textContent = product.type;
  document.getElementById("product-title").textContent = product.name;
  document.getElementById("product-price").textContent = `${money(product.price)} / ${product.unit}`;
  document.getElementById("coverage-label").textContent = `Coverage per ${capitalize(product.unit)}`;
  document.getElementById("packs-label").textContent = product.unit === "pack"
    ? "Required Packs"
    : `Required ${capitalize(plural(product.unit, 2))}`;
  document.getElementById("field-coverage").value = product.coveragePerPack;
  const note = document.getElementById("product-note");
  note.hidden = !product.note;
  note.textContent = product.note || "";
  await runCalculation(false);
}

async function runCalculation(addToCart) {
  const error = document.getElementById("result-error");
  error.hidden = true;
  const length = document.getElementById("field-length").value;
  const width = document.getElementById("field-width").value;
  const body = {
    productId: state.productId,
    length: length === "" ? null : Number(length),
    width: width === "" ? null : Number(width),
    storeId: document.getElementById("store-picker").value || "uk"
  };
  try {
    const data = await api(addToCart ? "/api/calculator/add-to-cart" : "/api/calculator/quote", {
      method: "POST",
      body: JSON.stringify(body)
    });
    document.getElementById("field-coverage").value = data.coveragePerPack;
    document.getElementById("result-area").textContent = `${formatNum(data.area)} m²`;
    document.getElementById("result-packs").textContent = String(data.quantity);
    renderNeed(data.lines);
    const formula = document.getElementById("result-formula");
    formula.hidden = false;
    formula.textContent = data.formula;
    if (addToCart)
      showView("cart");
  } catch (err) {
    error.hidden = false;
    error.textContent = `${err.data?.error || "Request failed"}\n${err.data?.detail || ""}`.trim();
    document.getElementById("result-need").hidden = true;
    document.getElementById("result-formula").hidden = true;
  }
}

function renderNeed(lines) {
  const box = document.getElementById("result-need");
  const parts = ['<p class="need-label">You need:</p>'];
  lines.forEach((line, index) => {
    if (index > 0)
      parts.push('<p class="plus">+</p>');
    parts.push(`<p class="need-line">${line.quantity} x ${escapeHtml(line.name)}</p>`);
  });
  box.innerHTML = parts.join("");
  box.hidden = false;
}

async function sendApi(tamper) {
  const status = document.getElementById("api-status");
  const output = document.getElementById("api-response");
  const cartNote = document.getElementById("api-cart");
  let payload;
  if (tamper) {
    payload = { productId: 100, length: 5, width: 4, quantity: 1, coverage: 100 };
    document.getElementById("api-request").value = JSON.stringify(payload, null, 2);
  } else {
    try {
      payload = JSON.parse(document.getElementById("api-request").value);
    } catch {
      status.textContent = "The request is not valid JSON.";
      return;
    }
  }

  const response = await fetch("/api/calculator/add-to-cart", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${state.token}`
    },
    body: JSON.stringify(payload)
  });
  const data = await response.json().catch(() => ({}));
  status.textContent = `HTTP ${response.status}`;
  output.textContent = JSON.stringify(data, null, 2);
  if (response.ok) {
    const cart = await api("/api/cart");
    cartNote.textContent = `Cart total is now ${money(cart.total)}. Posted quantity was not used.`;
  } else {
    const cart = await api("/api/cart");
    cartNote.textContent = `Cart total remains ${money(cart.total)}.`;
  }
}

async function loadCart() {
  const cart = await api("/api/cart");
  const empty = document.getElementById("cart-empty");
  const body = document.getElementById("cart-body");
  if (!cart.lines.length) {
    empty.hidden = false;
    body.hidden = true;
    return;
  }
  empty.hidden = true;
  body.hidden = false;
  document.getElementById("cart-lines").innerHTML = cart.lines.map((line) => `
    <div>
      <p class="cart-line">${line.quantity} x ${escapeHtml(line.name)}</p>
      <p class="cart-meta">${line.quantity} x ${money(line.unitPrice)}</p>
    </div>
  `).join("");
  document.getElementById("cart-total").textContent = money(cart.total);
}

async function loadSettings() {
  const settings = await api("/api/settings");
  document.getElementById("settings-stores").innerHTML = settings.stores.map((store) => `
    <label class="check">
      <input type="checkbox" data-store="${escapeHtml(store.id)}" ${store.enabled ? "checked" : ""}>
      ${escapeHtml(store.name)}
    </label>
  `).join("");
  document.getElementById("settings-enable").checked = settings.enableCalculator;
  document.getElementById("settings-message").hidden = true;
  syncEnableStatus();
}

function syncEnableStatus() {
  const enabled = document.getElementById("settings-enable").checked;
  const status = document.getElementById("settings-status");
  status.textContent = enabled ? "YES" : "NO";
  status.className = enabled ? "status-yes" : "status-no";
}

async function onSaveSettings(event) {
  event.preventDefault();
  const message = document.getElementById("settings-message");
  const stores = [...document.querySelectorAll("[data-store]")].map((input) => ({
    id: input.dataset.store,
    enabled: input.checked
  }));
  try {
    const result = await api("/api/settings", {
      method: "PUT",
      body: JSON.stringify({
        enableCalculator: document.getElementById("settings-enable").checked,
        stores
      })
    });
    message.hidden = false;
    message.classList.remove("error");
    message.textContent = `${result.message} Enable Calculator: ${result.status}`;
  } catch (err) {
    message.hidden = false;
    message.classList.add("error");
    message.textContent = err.data?.error || "Could not save settings.";
  }
}

async function onReset() {
  await api("/api/demo/reset", { method: "POST" });
  state.productId = 100;
  const message = document.getElementById("settings-message");
  message.hidden = false;
  message.classList.remove("error");
  message.textContent = "Demo data restored.";
  loadSettings();
}

async function runValidation() {
  const length = document.getElementById("val-length").value;
  const width = document.getElementById("val-width").value;
  const response = await fetch("/api/calculator/add-to-cart", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${state.token}`
    },
    body: JSON.stringify({
      productId: 100,
      length: length === "" ? null : Number(length),
      width: width === "" ? null : Number(width)
    })
  });
  const data = await response.json().catch(() => ({}));
  const result = document.getElementById("val-result");
  result.hidden = false;
  const cart = await api("/api/cart");
  if (response.ok) {
    result.classList.remove("error");
    result.textContent = `Server quantity: ${data.quantity}`;
    document.getElementById("val-raw").textContent =
      `HTTP ${response.status}\n${JSON.stringify(data, null, 2)}\n\nCart total is now ${money(cart.total)}.`;
    return;
  }
  result.classList.add("error");
  result.textContent = `${data.error}\n${data.detail}`;
  document.getElementById("val-raw").textContent =
    `HTTP ${response.status}\n${JSON.stringify(data, null, 2)}\n\nCart total remains ${money(cart.total)}.`;
}

function money(amount) {
  const value = Number(amount);
  return Number.isInteger(value) ? `$${value}` : `$${value.toFixed(2)}`;
}

function formatNum(amount) {
  const value = Number(amount);
  return Number.isInteger(value) ? String(value) : String(value);
}

function capitalize(value) {
  return value.charAt(0).toUpperCase() + value.slice(1);
}

function plural(unit, count) {
  return count === 1 ? unit : `${unit}s`;
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;");
}
