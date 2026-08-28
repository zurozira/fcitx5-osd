
#include <fcitx-utils/log.h>
#include <fcitx/addonfactory.h>
#include <fcitx/addonmanager.h>
#include <fcitx/instance.h>
#include <fcitx/inputcontext.h>

namespace fcitx {

class OsdBridge : public AddonInstance {
public:
    explicit OsdBridge(Instance *instance) : instance_(instance) {
        FCITX_INFO() << "OsdBridge constructed";
        handler_ = instance_->watchEvent(
            EventType::InputContextSwitchInputMethod, EventWatcherPhase::Default, [this](Event &event) {
                auto &icEvent = static_cast<InputContextEvent &>(event);
                FCITX_INFO() << "Switched IM on context, new IM: "
                             << instance_->inputMethod(icEvent.inputContext());
            });
    }

private:
    Instance *instance_;
    std::unique_ptr<HandlerTableEntry<EventHandler>> handler_;
};

class OsdBridgeFactory : public AddonFactory {
    AddonInstance *create(AddonManager *manager) override {
        return new OsdBridge(manager->instance());
    }
};

}

FCITX_ADDON_FACTORY(fcitx::OsdBridgeFactory)
