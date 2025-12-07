// *****************************************************************************
// Header file for BrowserPalette class (Dimension Gh Add-On)
// *****************************************************************************

#ifndef BROWSERPALETTE_HPP
#define BROWSERPALETTE_HPP

// ---------------------------------- Includes ---------------------------------

#include "APIEnvir.h"
#include "ACAPinc.h"		// also includes APIdefs.h
#include "DGModule.hpp"
#include "DGBrowser.hpp"

#define BrowserPaletteResId 32500
#define BrowserPaletteMenuResId 32500
#define BrowserPaletteMenuItemIndex 1

// --- Class declaration: BrowserPalette ------------------------------------------

class BrowserPalette final : public DG::Palette,
							 public DG::PanelObserver
{
protected:
	enum {
		BrowserId = 1
	};

	DG::Browser		browser;

	void InitBrowserControl ();
	void RegisterACAPIJavaScriptObject ();
	void SetMenuItemCheckedState (bool);

	virtual void PanelResized (const DG::PanelResizeEvent& ev) override;
	virtual	void PanelCloseRequested (const DG::PanelCloseRequestEvent& ev, bool* accepted) override;

	static GSErrCode PaletteControlCallBack (Int32 paletteId, API_PaletteMessageID messageID, GS::IntPtr param);

	static GS::Ref<BrowserPalette> instance;

	BrowserPalette ();

public:
	virtual ~BrowserPalette ();

	static bool				HasInstance ();
	static void				CreateInstance ();
	static BrowserPalette&	GetInstance ();
	static void				DestroyInstance ();

	void Show ();
	void Hide ();

	static GSErrCode				RegisterPaletteControlCallBack ();
};

#endif // BROWSERPALETTE_HPP
