export function normalizarNombreCliente(nombre) {
  return String(nombre || '')
    .replace(/[^\p{L}\s]/gu, '')
    .replace(/\s+/g, ' ')
    .replace(/(^|\s)(\p{L})/gu, (_, espacio, letra) => espacio + letra.toLocaleUpperCase())
    .replace(/(?<=\p{L})(\p{L})/gu, (letra) => letra.toLocaleLowerCase());
}

export function normalizarTelefono(telefono) {
  return String(telefono || '').replace(/\D/g, '').slice(0, 15);
}
