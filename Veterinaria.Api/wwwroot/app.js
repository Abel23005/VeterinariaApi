(() => {
  const $ = (s, root = document) => root.querySelector(s);
  const $$ = (s, root = document) => [...root.querySelectorAll(s)];
  const state = { token: localStorage.getItem('vetcareToken'), user: null, pets: [], owners: [], view: 'inicio' };
  const authScreen = $('#authScreen'), shell = $('#appShell'), notice = $('#notice');
  const roleIsVet = () => state.user?.rol === 'Veterinario';
  const esc = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  const initial = value => (String(value || 'V').trim()[0] || 'V').toUpperCase();
  const api = async (path, options = {}) => {
    const headers = { ...(options.body ? { 'Content-Type': 'application/json' } : {}), ...(state.token ? { Authorization: `Bearer ${state.token}` } : {}), ...(options.headers || {}) };
    const response = await fetch(`/api/${path}`, { ...options, headers });
    if (!response.ok) {
      let message = `No se pudo completar la solicitud (${response.status}).`;
      try { const body = await response.json(); message = body.error || body.title || message; } catch { /* el cuerpo puede venir vacío */ }
      const error = new Error(message); error.status = response.status; throw error;
    }
    if (response.status === 204) return null;
    return response.json();
  };
  const toast = message => { const el = $('#toast'); el.textContent = message; el.classList.add('show'); clearTimeout(toast.timer); toast.timer = setTimeout(() => el.classList.remove('show'), 3000); };
  const showNotice = (message, kind = 'error') => { notice.textContent = message; notice.className = `notice ${kind}`; notice.hidden = false; };
  const clearNotice = () => { notice.hidden = true; notice.textContent = ''; };
  const signOut = () => { state.token = null; state.user = null; localStorage.removeItem('vetcareToken'); shell.hidden = true; authScreen.hidden = false; $('#loginForm').reset(); };
  const enterApp = async () => {
    state.user = await api('auth/perfil');
    authScreen.hidden = true; shell.hidden = false; clearNotice();
    $('#userEmail').textContent = state.user.correo;
    $('#userRole').textContent = state.user.rol;
    $('#userAvatar').textContent = initial(state.user.correo);
    $('#topAvatar').textContent = initial(state.user.correo);
    $('#greeting').innerHTML = `Hola, ${esc(state.user.correo.split('@')[0])} <span>✦</span>`;
    $('#todayLabel').textContent = new Intl.DateTimeFormat('es-CO', { weekday: 'short', day: 'numeric', month: 'short' }).format(new Date());
    await refreshData();
  };
  const refreshData = async () => {
    try {
      const [pets, owners] = await Promise.all([api('mascotas'), api('duenos')]);
      state.pets = pets; state.owners = owners; renderAll();
    } catch (error) {
      if (error.status === 401) { signOut(); return; }
      showNotice(error.message || 'No fue posible cargar los datos de la clínica.');
    }
  };
  const petIcon = species => /gato|felin/i.test(species) ? 'cat' : /ave|pájaro|pajaro/i.test(species) ? 'bird' : '';
  const petGlyph = species => /gato|felin/i.test(species) ? '🐱' : /ave|pájaro|pajaro/i.test(species) ? '🐦' : '🐶';
  const formatDate = value => { if (!value) return '—'; const date = new Date(`${String(value).slice(0, 10)}T12:00:00`); return Number.isNaN(date.valueOf()) ? esc(value) : new Intl.DateTimeFormat('es-CO', { day: '2-digit', month: 'short', year: 'numeric' }).format(date); };
  const ownerFor = pet => state.owners.find(o => o.duenoId === pet.duenoId);
  const petMarkup = (pet, compact = false) => {
    const owner = ownerFor(pet);
    const actions = roleIsVet() ? `<div class="row-actions"><button class="row-action" data-edit-pet="${pet.mascotaId}">Editar</button><button class="row-action danger" data-delete-pet="${pet.mascotaId}">Eliminar</button></div>` : '';
    return `<tr><td><div class="pet-name-cell"><span class="pet-avatar ${petIcon(pet.especie)}">${petGlyph(pet.especie)}</span>${esc(pet.nombre)}</div></td><td>${esc(pet.especie)}</td>${compact ? '' : `<td>${esc(pet.raza)}</td>`}<td><div class="owner-mini"><span class="owner-dot">${initial(owner?.nombre || pet.nombreDueno)}</span>${esc(owner?.nombre || pet.nombreDueno || '—')}</div></td><td>${formatDate(pet.fechaNacimiento)}</td>${compact ? '<td></td>' : `<td>${actions}</td>`}</tr>`;
  };
  const renderDashboard = () => {
    $('#petCount').textContent = state.pets.length;
    $('#ownerCount').textContent = state.owners.length;
    $('#speciesCount').textContent = new Set(state.pets.map(p => (p.especie || '').toLocaleLowerCase()).filter(Boolean)).size;
    const recent = [...state.pets].sort((a, b) => String(b.mascotaId).localeCompare(String(a.mascotaId), undefined, { numeric: true })).slice(0, 5);
    $('#recentPets').innerHTML = recent.length ? recent.map(p => petMarkup(p, true)).join('') : '<tr><td colspan="5" class="empty-cell">Aún no hay mascotas. Registra tu primer paciente.</td></tr>';
  };
  const renderPets = () => {
    const q = $('#petSearch').value.trim().toLocaleLowerCase(); const species = $('#speciesFilter').value;
    const found = state.pets.filter(p => (!species || p.especie === species) && `${p.nombre} ${p.especie} ${p.raza} ${p.nombreDueno}`.toLocaleLowerCase().includes(q));
    $('#petResultCount').textContent = `${found.length} ${found.length === 1 ? 'paciente' : 'pacientes'}`;
    $('#petRows').innerHTML = found.length ? found.map(p => petMarkup(p)).join('') : '<tr><td colspan="6" class="empty-cell">No encontramos mascotas con esa búsqueda.</td></tr>';
    const select = $('#speciesFilter'); const selected = select.value; const items = [...new Set(state.pets.map(p => p.especie).filter(Boolean))].sort((a,b) => a.localeCompare(b));
    select.innerHTML = '<option value="">Todas las especies</option>' + items.map(x => `<option value="${esc(x)}">${esc(x)}</option>`).join(''); select.value = items.includes(selected) ? selected : '';
  };
  const renderOwners = () => {
    const q = $('#ownerSearch').value.trim().toLocaleLowerCase(); const found = state.owners.filter(o => `${o.nombre} ${o.correo} ${o.telefono}`.toLocaleLowerCase().includes(q));
    $('#ownerResultCount').textContent = `${found.length} ${found.length === 1 ? 'familia' : 'familias'}`;
    $('#ownerGrid').innerHTML = found.length ? found.map(o => `<article class="owner-card"><div class="owner-card-top"><div class="owner-avatar">${initial(o.nombre)}</div><div class="owner-info"><b>${esc(o.nombre)}</b><small>Familia VetCare</small></div></div><div class="owner-contact">✉ &nbsp;${esc(o.correo)}<br><br>⌕ &nbsp;${esc(o.telefono)}</div><div class="owner-card-foot"><span class="pet-badge">♡ &nbsp;${o.cantidadMascotas} ${o.cantidadMascotas === 1 ? 'mascota' : 'mascotas'}</span><div class="card-actions"><button class="row-action" data-edit-owner="${o.duenoId}">Editar</button>${roleIsVet() ? `<button class="row-action danger" data-delete-owner="${o.duenoId}">Eliminar</button>` : ''}</div></div></article>`).join('') : '<div class="empty-cell">No encontramos familias con esa búsqueda.</div>';
  };
  const renderAll = () => { renderDashboard(); renderPets(); renderOwners(); };
  const setView = name => {
    state.view = name; $$('.view').forEach(v => v.classList.toggle('active-view', v.id === `view-${name}`));
    $$('.nav-link').forEach(b => b.classList.toggle('active', b.dataset.view === name));
    const labels = { inicio: 'Resumen', mascotas: 'Mascotas', duenos: 'Dueños' }; $('#crumb').textContent = labels[name] || 'Resumen';
    document.title = `${labels[name] || 'Resumen'} · VetCare`;
  };
  const openPet = pet => {
    const form = $('#petForm'); form.reset(); form.elements.id.value = pet?.mascotaId || '';
    $('#petDialogTitle').textContent = pet ? 'Editar mascota' : 'Registrar mascota';
    const ownerSelect = form.elements.duenoId; ownerSelect.innerHTML = state.owners.map(o => `<option value="${o.duenoId}">${esc(o.nombre)}</option>`).join('');
    if (!state.owners.length) { toast('Primero registra un dueño para asociar a la mascota.'); return; }
    if (pet) { form.elements.nombre.value = pet.nombre; form.elements.especie.value = pet.especie; form.elements.raza.value = pet.raza; form.elements.fechaNacimiento.value = String(pet.fechaNacimiento).slice(0,10); ownerSelect.value = pet.duenoId; }
    $('#petDialog').showModal();
  };
  const openOwner = owner => {
    const form = $('#ownerForm'); form.reset(); form.elements.id.value = owner?.duenoId || '';
    $('#ownerDialogTitle').textContent = owner ? 'Editar dueño' : 'Registrar dueño';
    if (owner) { form.elements.nombre.value = owner.nombre; form.elements.correo.value = owner.correo; form.elements.telefono.value = owner.telefono; }
    $('#ownerDialog').showModal();
  };
  const errorToast = error => toast(error.message || 'Ocurrió un error.');

  $$('#loginFormWrap [data-show-register], #registerFormWrap [data-show-login]').forEach(button => button.addEventListener('click', () => {
    const showRegister = button.hasAttribute('data-show-register'); $('#loginFormWrap').hidden = showRegister; $('#registerFormWrap').hidden = !showRegister;
  }));
  $('#loginForm').addEventListener('submit', async event => {
    event.preventDefault(); const data = Object.fromEntries(new FormData(event.currentTarget));
    try { const result = await api('auth/login', { method: 'POST', body: JSON.stringify(data) }); state.token = result.token; localStorage.setItem('vetcareToken', result.token); await enterApp(); }
    catch (error) { showNotice(error.status === 401 ? 'El correo o la contraseña no coinciden.' : error.message); }
  });
  $('#registerForm').addEventListener('submit', async event => {
    event.preventDefault(); const data = Object.fromEntries(new FormData(event.currentTarget));
    try { const result = await api('auth/registro', { method: 'POST', body: JSON.stringify(data) }); state.token = result.token; localStorage.setItem('vetcareToken', result.token); await enterApp(); toast('¡Tu cuenta está lista!'); }
    catch (error) { showNotice(error.status === 409 ? 'Este correo ya está registrado. Inicia sesión.' : error.message); }
  });
  $('#logoutBtn').addEventListener('click', signOut);
  $$('.nav-link, [data-view]').forEach(button => button.addEventListener('click', () => setView(button.dataset.view)));
  $('#quickPetBtn').addEventListener('click', () => openPet()); $('#addPetBtn').addEventListener('click', () => openPet()); $('#addOwnerBtn').addEventListener('click', () => openOwner());
  $('#petSearch').addEventListener('input', renderPets); $('#speciesFilter').addEventListener('change', renderPets); $('#ownerSearch').addEventListener('input', renderOwners);
  $$('.close-dialog').forEach(button => button.addEventListener('click', () => button.closest('dialog').close()));
  $('#petForm').addEventListener('submit', async event => {
    event.preventDefault(); const form = event.currentTarget; const data = Object.fromEntries(new FormData(form)); const id = data.id; delete data.id; data.duenoId = Number(data.duenoId);
    try { if (id) await api(`mascotas/${id}`, { method: 'PUT', body: JSON.stringify(data) }); else await api('mascotas', { method: 'POST', body: JSON.stringify(data) }); $('#petDialog').close(); await refreshData(); toast(id ? 'Mascota actualizada.' : 'Mascota registrada.'); }
    catch (error) { errorToast(error); }
  });
  $('#ownerForm').addEventListener('submit', async event => {
    event.preventDefault(); const form = event.currentTarget; const data = Object.fromEntries(new FormData(form)); const id = data.id; delete data.id;
    try { if (id) await api(`duenos/${id}`, { method: 'PUT', body: JSON.stringify(data) }); else await api('duenos', { method: 'POST', body: JSON.stringify(data) }); $('#ownerDialog').close(); await refreshData(); toast(id ? 'Dueño actualizado.' : 'Familia registrada.'); }
    catch (error) { errorToast(error); }
  });
  document.addEventListener('click', async event => {
    const editPet = event.target.closest('[data-edit-pet]'); const deletePet = event.target.closest('[data-delete-pet]'); const editOwner = event.target.closest('[data-edit-owner]'); const deleteOwner = event.target.closest('[data-delete-owner]');
    if (editPet) { const item = state.pets.find(p => p.mascotaId === Number(editPet.dataset.editPet)); if (item) openPet(item); }
    if (deletePet && confirm('¿Eliminar este registro de mascota?')) { try { await api(`mascotas/${deletePet.dataset.deletePet}`, { method: 'DELETE' }); await refreshData(); toast('Registro eliminado.'); } catch (error) { errorToast(error); } }
    if (editOwner) { const item = state.owners.find(o => o.duenoId === Number(editOwner.dataset.editOwner)); if (item) openOwner(item); }
    if (deleteOwner && confirm('¿Eliminar este dueño? Los dueños con mascotas asociadas no se pueden eliminar.')) { try { await api(`duenos/${deleteOwner.dataset.deleteOwner}`, { method: 'DELETE' }); await refreshData(); toast('Dueño eliminado.'); } catch (error) { errorToast(error); } }
  });
  const start = async () => {
    try { if (!state.token) { authScreen.hidden = false; shell.hidden = true; return; } await enterApp(); }
    catch { signOut(); }
  };
  start();
})();
