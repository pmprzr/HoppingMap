"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
/** Fixed display order for leagues, used both for filters and the match list. */
const leagueOrder = ["bl1", "bl2", "bl3"];
/** Maps OpenLigaDB league shortcuts to human-readable German league names. */
const leagueNames = {
    bl1: "1. Bundesliga",
    bl2: "2. Bundesliga",
    bl3: "3. Liga"
};
/**
 * Maps JavaScript's Date.getDay() result (0 = Sunday ... 6 = Saturday)
 * to a display name and a marker/legend color for that weekday.
 */
const weekdayColors = {
    0: { name: "Sonntag", color: "#e6194B" },
    1: { name: "Montag", color: "#3cb44b" },
    2: { name: "Dienstag", color: "#ffe119" },
    3: { name: "Mittwoch", color: "#4363d8" },
    4: { name: "Donnerstag", color: "#f58231" },
    5: { name: "Freitag", color: "#911eb4" },
    6: { name: "Samstag", color: "#42d4f4" }
};
/** All matches loaded from the API, unfiltered. Populated once by loadMatches(). */
let allMatches = [];
/** The Leaflet markers currently drawn on the map, tracked so they can be removed before a re-render. */
let currentMarkers = [];
/** League shortcuts currently enabled via the checkboxes. Starts with all leagues selected. */
let selectedLeagues = new Set(leagueOrder);
/**
 * Fetches all current-matchday matches from the backend API,
 * then renders the weekday legend and the initial (unfiltered) view.
 * Called once on page load.
 */
function loadMatches() {
    return __awaiter(this, void 0, void 0, function* () {
        const response = yield fetch("/api/matches");
        if (!response.ok) {
            console.error("Fehler beim Laden der Spiele");
            return;
        }
        allMatches = yield response.json();
        renderLegend();
        applyFilter();
    });
}
/**
 * Builds the league filter checkboxes (Bootstrap-styled) and wires up
 * their change events to update selectedLeagues and re-render.
 * Called once on page load, independent of whether match data has arrived yet.
 */
function setupFilters() {
    var _a;
    const container = document.getElementById("league-filter");
    if (!container) {
        return;
    }
    container.innerHTML = "";
    const strong = document.createElement("strong");
    strong.className = "me-2";
    strong.textContent = "Ligen:";
    container.appendChild(strong);
    for (const league of leagueOrder) {
        const wrapper = document.createElement("div");
        wrapper.className = "form-check form-check-inline";
        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.className = "form-check-input";
        checkbox.id = `league-${league}`;
        checkbox.checked = true;
        checkbox.addEventListener("change", () => {
            if (checkbox.checked) {
                selectedLeagues.add(league);
            }
            else {
                selectedLeagues.delete(league);
            }
            applyFilter();
        });
        const label = document.createElement("label");
        label.className = "form-check-label";
        label.htmlFor = checkbox.id;
        label.textContent = (_a = leagueNames[league]) !== null && _a !== void 0 ? _a : league;
        wrapper.appendChild(checkbox);
        wrapper.appendChild(label);
        container.appendChild(wrapper);
    }
}
/**
 * Filters allMatches down to the currently selected leagues and triggers a re-render.
 * This is the single entry point called whenever the filter selection changes.
 */
function applyFilter() {
    const filtered = allMatches.filter(m => selectedLeagues.has(m.league));
    render(filtered);
}
/** Returns the legend/marker color for the weekday of the given ISO date string. */
function getWeekdayColor(dateStr) {
    const day = new Date(dateStr).getDay();
    return weekdayColors[day].color;
}
/** Builds a Google Maps directions URL pointing to the match's stadium coordinates. */
function getRouteUrl(match) {
    return `https://www.google.com/maps/dir/?api=1&destination=${match.latitude},${match.longitude}`;
}
/**
 * Builds the HTML for a match's action links (route, optionally stadium/ticket links),
 * used both in map popups and in the match list. Europlan/ticket links are only
 * included when the backend actually provided a non-empty URL for that stadium.
 */
function buildLinksHtml(match) {
    const links = [];
    links.push(`<a href="${getRouteUrl(match)}" target="_blank" rel="noopener">Route</a>`);
    if (match.europlanUrl) {
        links.push(`<a href="${match.europlanUrl}" target="_blank" rel="noopener">Stadion (Europlan)</a>`);
    }
    if (match.ticketShopUrl) {
        links.push(`<a href="${match.ticketShopUrl}" target="_blank" rel="noopener">Tickets</a>`);
    }
    return links.join(" · ");
}
/**
 * Redraws both the map markers and the match list for the given set of matches.
 * This is the shared entry point called after loading or after a filter change,
 * so the map and the list always stay in sync with each other.
 */
function render(matches) {
    renderMarkers(matches);
    renderList(matches);
}
/**
 * Clears any previously drawn markers and draws one circle marker per match,
 * colored by weekday, with a popup showing match details and action links.
 */
function renderMarkers(matches) {
    for (const marker of currentMarkers) {
        map.removeLayer(marker);
    }
    currentMarkers = [];
    for (const match of matches) {
        const color = getWeekdayColor(match.startTime);
        const marker = L.circleMarker([match.latitude, match.longitude], {
            radius: 8,
            color: color,
            fillColor: color,
            fillOpacity: 0.9,
            weight: 2
        })
            .addTo(map)
            .bindPopup(`
            <strong>${match.homeTeam} - ${match.awayTeam}</strong>
            <br>
            ${match.stadium}
            <br>
            ${new Date(match.startTime).toLocaleString("de-DE")}
            <br>
            ${buildLinksHtml(match)}
        `);
        currentMarkers.push(marker);
    }
}
/**
 * Rebuilds the sidebar match list, grouped by league (in leagueOrder) with a
 * Bootstrap list-group per league. Leagues with no matches are skipped entirely.
 * Each list item shows kickoff time, teams, a weekday-colored left border, and action links.
 */
function renderList(matches) {
    var _a;
    const container = document.getElementById("match-list");
    if (!container) {
        return;
    }
    container.innerHTML = "";
    for (const league of leagueOrder) {
        const matchesInLeague = matches.filter(m => m.league === league);
        if (matchesInLeague.length === 0) {
            continue;
        }
        const heading = document.createElement("h5");
        heading.className = "mt-3";
        heading.textContent = (_a = leagueNames[league]) !== null && _a !== void 0 ? _a : league;
        container.appendChild(heading);
        const list = document.createElement("ul");
        list.className = "list-group mb-3";
        for (const match of matchesInLeague) {
            const item = document.createElement("li");
            item.className = "list-group-item";
            item.style.borderLeftWidth = "4px";
            item.style.borderLeftColor = getWeekdayColor(match.startTime);
            const text = document.createElement("div");
            text.textContent =
                `${new Date(match.startTime).toLocaleString("de-DE")} — ${match.homeTeam} vs. ${match.awayTeam}`;
            item.appendChild(text);
            const linksDiv = document.createElement("div");
            linksDiv.className = "small mt-1";
            linksDiv.innerHTML = buildLinksHtml(match);
            item.appendChild(linksDiv);
            list.appendChild(item);
        }
        container.appendChild(list);
    }
}
/**
 * Renders the static weekday color legend (Sunday..Saturday) once.
 * Independent of match data and of the current filter selection.
 */
function renderLegend() {
    const container = document.getElementById("legend");
    if (!container) {
        return;
    }
    container.innerHTML = "";
    const strong = document.createElement("strong");
    strong.className = "me-2";
    strong.textContent = "Wochentage:";
    container.appendChild(strong);
    for (const day of Object.values(weekdayColors)) {
        const item = document.createElement("span");
        item.className = "legend-item";
        const swatch = document.createElement("span");
        swatch.className = "weekday-swatch";
        swatch.style.backgroundColor = day.color;
        item.appendChild(swatch);
        item.appendChild(document.createTextNode(day.name));
        container.appendChild(item);
    }
}
// Entry point: build the filter UI immediately, then start loading match data.
setupFilters();
loadMatches();
//# sourceMappingURL=matches.js.map